using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using VideoLibrary;

DrawApplicationLogo();

CConsole.Note("Spotisharp v" + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString());
CConsole.Note("\tCopyright \u00a92021 Damian Ziolo");

if (!ConfigManager.Init())
{
    CConsole.Error("Couldn't load or create configuration file");
    return;
}

ConfigManager.Properties.EnsureDirsExist();

if (!FFmpegResolver.IsFFmpegInstalled())
{
    CConsole.Error("FFmpeg is missing");
    return;
}

string input = string.Empty;

if (args.Length == 0)
{
    CConsole.Warn("No arguments provided. Awaiting for input");
    input = CConsole.ReadInput();
}
else
{
    input = args[0];
}

if(input == string.Empty)
{
    CConsole.Error("Input has to contain something");
    return;
}

CConsole.Debug("Input: " + input);

CConsole.Info("Logging to Spotify");
SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null) 
{
    CConsole.Error("Couldn't sign in to Spotify. Exiting");
    return;
};

CConsole.Info("Logged in successfully");

SpotifyUriType uriType = SpotifyUriResolver.GetUriType(input);
SpotifyBrowseCategory category = SpotifyUriResolver.GetBrowseCategory(input);
ConcurrentBag<TrackInfoModel> trackInfoBag = new ConcurrentBag<TrackInfoModel>();
string queryID = SpotifyUriResolver.GetID(input, uriType);

if (queryID != string.Empty)
{
    input = queryID;
}

switch (category)
{
    case SpotifyBrowseCategory.None:
    case SpotifyBrowseCategory.Track:
        CConsole.Info("User request type: SingleTrack");
        CConsole.Info("Queueing track...");
        await SpotifyService.PackSingleTrack(client, input, trackInfoBag, uriType);
        break;

    case SpotifyBrowseCategory.Playlist:
        CConsole.Info("User request type: Playlist");
        CConsole.Info("Queueing tracks... (It might take some time)");
        await SpotifyService.PackPlaylistTracks(client, input, trackInfoBag);
        break;

    case SpotifyBrowseCategory.Album:
        CConsole.Info("User request type: Album");
        CConsole.Info("Queueing tracks... (It might take some time)");
        await SpotifyService.PackAlbumTracks(client, input, trackInfoBag);
        break;
}

int workersCount = ConfigManager.Properties.WorkersCount;

if(workersCount < 1 || workersCount > 6)
{
    CConsole.Warn("WorkersCount has to be set in range of 1-6. Changing to 4");
    workersCount = 4;
}

for (int i = 0; i < workersCount; i++)
{
    CConsole.Note("Waiting for task...");
}

int topCursorPosition = Console.CursorTop - workersCount;

CConsole.Note("Press CTRL-C to abort");

await Task.WhenAll(Enumerable.Range(0, workersCount).Select(async workerId =>
{
    int positionY = topCursorPosition + workerId;
    while (trackInfoBag.TryTake(out TrackInfoModel? trackInfo))
    {
        string safeArtistName = FileSystemResolver.ReplaceForbiddenChars(trackInfo.Artist);
        string safeTitle = FileSystemResolver.ReplaceForbiddenChars(trackInfo.Title);
        string fullName = safeArtistName + " - " + safeTitle;

        DirectoryInfo trackDir = Directory.CreateDirectory
                (
                    Path.Combine
                    (
                        ConfigManager.Properties.MusicDirectory,
                        FileSystemResolver.ReplaceForbiddenChars(trackInfo.Playlist)
                    )
                );

        string convertedFilePath = Path.Combine(trackDir.FullName, fullName) + ".mp3";

        if (File.Exists(convertedFilePath))
        {
            CConsole.Overwrite($"Worker #{workerId} ::: Skipping: {fullName}", positionY,CConsoleType.Info);
            await Task.Delay(250);
            continue;
        }

        CConsole.Debug($"Worker #{workerId} ::: Searching lyrics ::: {fullName}");
        Task<string> lyricsTask = 
            MusixmatchService.SearchLyricsFromText(fullName);

        CConsole.Debug($"Worker #{workerId} ::: Searching Youtube ::: {fullName}");
        string[] results = await YoutubeService.SearchByText(fullName, 3);

        YouTubeVideo? audioTrack = null;

        for(int i = 0; i < results.Length; i++)
        {
            if(results[i] == string.Empty)
            {
                continue;
            }

            audioTrack = await YoutubeService.GetAudioOnly(results[i]);

            if (audioTrack != null)
            {
                break;
            }   
        }

        if(audioTrack == null)
        {
            CConsole.Overwrite
            (
                $"Worker #{workerId} ::: Failed to retrieve audiostream for: {fullName}",
                positionY,
                CConsoleType.Warn
            );
            await Task.Delay(1000);
            continue;
        }

        CConsole.Debug($"Worker #{workerId} ::: Downloading ::: {fullName}");

        Task<byte[]> albumPictureTask = Task.Run(async () =>
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetByteArrayAsync(trackInfo.AlbumPicture);
            }
        });

        Stream audioStream = await YoutubeService.GetStreamAsync(audioTrack.Uri,
            new Progress<Tuple<long, long>>(pValues =>
            {
                int percentage = (int)Math.Ceiling(100.0 * pValues.Item1 / pValues.Item2);

                CConsole.Overwrite
                (
                    string.Format
                    (
                        "Worker #{0} ::: {1}{2} {3}% Q:{4} ::: {5}",
                        workerId,
                        new string('█', (int)(percentage / 5)),
                        new string('▓', 20 - (int)(percentage / 5)),
                        percentage.ToString("D3"),
                        trackInfoBag.Count.ToString("D3"),
                        fullName
                    ),
                    positionY,
                    CConsoleType.Info,
                    false
                );
            }));
        
        CConsole.Debug($"Worker #{workerId} ::: Converting ::: {fullName}");

        using (audioStream)
        {
            using (Process ffProcess = new Process())
            {
                ffProcess.StartInfo.FileName = "ffmpeg";
                ffProcess.StartInfo.Arguments = $"-i - -q:a 0 \"{convertedFilePath}\"";
                ffProcess.StartInfo.UseShellExecute = false;
                ffProcess.StartInfo.CreateNoWindow = true;
                ffProcess.StartInfo.RedirectStandardError = false;
                ffProcess.StartInfo.RedirectStandardInput = true;
                ffProcess.Start();
                Stream ffInputStream = ffProcess.StandardInput.BaseStream;
                audioStream.Seek(0, SeekOrigin.Begin);
                audioStream.CopyTo(ffInputStream);
                ffInputStream.Close();
                ffProcess.WaitForExit();
            }
        }

        CConsole.Debug($"Worker #{workerId} ::: Writing Metadata ::: {fullName}");

        using (TagLib.File file = TagLib.File.Create(convertedFilePath))
        {
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            file.Tag.Performers = new string[] { trackInfo.Artist };
            file.Tag.AlbumArtists = new string[] { trackInfo.Artist };
            file.Tag.Composers = new string[] { trackInfo.Artist };
            file.Tag.Copyright = trackInfo.Copyright;
            file.Tag.Lyrics = await lyricsTask;
            file.Tag.Title = trackInfo.Title;
            file.Tag.Album = trackInfo.Album;
            file.Tag.Track = Convert.ToUInt32(trackInfo.TrackNumber);
            file.Tag.Disc = Convert.ToUInt32(trackInfo.DiscNumber);
            file.Tag.Year = Convert.ToUInt32(trackInfo.Year);
            file.Tag.Comment = trackInfo.Url;
            file.Tag.Genres = new string[] { trackInfo.Genres };
            file.Tag.Pictures = new TagLib.Picture[]
            {
                new TagLib.Picture(await albumPictureTask)
            };
            file.Save();
        }

        CConsole.Debug($"Worker #{workerId} ::: Finished writing metadata ::: {fullName}");
    }
    CConsole.Overwrite($"Worker #{workerId} ::: Finished all tasks", positionY, CConsoleType.Info);
}));

Console.SetCursorPosition(0, topCursorPosition + workersCount + 1);


void DrawApplicationLogo()
{
    string[] logo =
    {
        "        ▓        ▓",
        "        ▓        ▓      ▄▄",
        "  ▄▄▄███▓████████▓▄▄▓▓▀▀▀",
        " ████████████████▓█████▄▄",
        "     ▄▄▄▓▀▀▀     ▓ ▀▀█████",
        "▄████████████████▓▄▄",
        "  ▀█▀▀▀▀▓▀▀▀▀▀▀▀██████▄",
        "      ▄▄▓▄▄▄▄▄   ▓  ▀▀█▀",
        "   █████▓█▀██████▓█▄",
        "        ▓        ▓▀██",
        "        ▓        ▓",
        "        ▓        ▓",
        "        ▀        ▀"
    };

    for (int i = 0; i < logo.Length; i++)
    {
        int rowCharsCount = logo[i].Length;
        string[] ansiColoredChars = new string[rowCharsCount];
        for (int j = 0; j < rowCharsCount; j++)
        {
            char c = logo[i][j];
            if (c == '▓' || c == '▀' || c == '▄')
            {
                ansiColoredChars[j] = "\u001b[38;2;99;96;97m" + c + "\u001b[0m";
            }
            else
            {
                ansiColoredChars[j] = "\u001b[38;2;155;11;7m" + c + "\u001b[0m";
            }
        }
        CConsole.Note(string.Join("", ansiColoredChars));
    }
}