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

if (!FFmpegResolver.IsFFmpegInstalled())
{
    CConsole.Error("FFmpeg is missing");
    return;
}

if (!ConfigManager.Init())
{
    CConsole.Error("Couldn't load or create configuration file");
    return;
}

ConfigManager.Properties.EnsureDirsExist();

if (ConfigManager.Properties.IsFirstTime)
{
    CConsole.Note("Hey!, this is your first time eh? Lemme tell ya somethin important");
    CConsole.Warn("Spotisharp is free and opensource software and it always will be");
    CConsole.Warn("I'm treating this project as a hobby not as a duty");
    CConsole.Warn("Stuff might get broken, some things might not work at all");
    CConsole.Warn("Also make sure you're using official software");
    CConsole.Note("Any collaboration will be appreciated. This includes features, bugfixes etc.");
    CConsole.Note("Okay enough for now, lets get to work");
    ConfigManager.Properties.IsFirstTime = false;
    ConfigManager.WriteChanges();
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
    return;
}

CConsole.Info("Logging to Spotify");
SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null) 
{
    CConsole.Error("Couldn't sign in to Spotify. Exiting");
    return;
};
CConsole.Info("Logged in successfully");

if(input == null) return;

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
        TrackInfoModel? trackInfo = await SpotifyService.GetSingleTrack(client, input, uriType);
        if (trackInfo == null) 
        {
            CConsole.Error("Spotify returned 0 matches. Exiting.");
            return;
        };
        trackInfoBag.Add(trackInfo);
        break;

    case SpotifyBrowseCategory.Playlist:
        CConsole.Info("User request type: Playlist");
        CConsole.Info("Queueing tracks...");
        await SpotifyService.PackPlaylistTracks(client, input, trackInfoBag);
        break;

    case SpotifyBrowseCategory.Album:
        CConsole.Info("User request type: Album");
        CConsole.Info("Queueing tracks...");
        await SpotifyService.PackAlbumTracks(client, input, trackInfoBag);
        break;
}

ConcurrentBag<YouTubeVideo> audioStreams = new ConcurrentBag<YouTubeVideo>();

int topCursorPosition = Console.CursorTop;
int workersCount = 4;

await Task.WhenAll(Enumerable.Range(0, workersCount).Select(async workerId =>
{
    int positionY = topCursorPosition + workerId;
    while (trackInfoBag.TryTake(out TrackInfoModel trackInfo))
    {
        string safeArtistName = FileSystemResolver.ReplaceForbiddenChars(trackInfo.Artist);
        string safeTitle = FileSystemResolver.ReplaceForbiddenChars(trackInfo.Title);
        string fullName = safeArtistName + " - " + safeTitle;

        DirectoryInfo trackDir = Directory.CreateDirectory
                (
                    Path.Combine
                    (
                        ConfigManager.Properties.MusicDirectory,
                        trackInfo.Playlist
                    )
                );

        //Task<string> lyricsTask = 
            //MusixmatchService.SearchLyricsFromText(fullName);

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

        Stream audioStream = await YoutubeService.GetStreamAsync(audioTrack.Uri,
            new Progress<Tuple<long, long>>(pValues =>
            {
                int percentage = (int)Math.Ceiling(100.0 * pValues.Item1 / pValues.Item2);

                CConsole.Overwrite
                (
                    string.Format
                    (
                        "Worker #{0} ::: |{1}{2}| {3}% Q:{4} ::: {5}",
                        workerId,
                        new string('█', (int)(percentage / 5)),
                        new string('▓', 20 - (int)(percentage / 5)),
                        percentage.ToString("D3"),
                        trackInfoBag.Count.ToString("D3"),
                        fullName
                    ),
                    positionY,
                    CConsoleType.Info
                );
            }));

        using (audioStream)
        {
            using (Process ffProcess = new Process())
            {
                ffProcess.StartInfo.FileName = "ffmpeg";
                ffProcess.StartInfo.Arguments = $"-i - -q:a 0 \"{Path.Combine(trackDir.FullName, fullName)}.mp3\"";
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
    }
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