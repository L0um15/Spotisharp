using SpotifyAPI.Web;
using Spotisharp.Client.ColoredConsole;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;
using System.Reflection;
using VideoLibrary;

CConsole.WriteLine("Spotisharp v" + 
    Assembly.GetExecutingAssembly().GetName().Version!.ToString());

CConsole.WriteLine($"(\u00a9) 2020-2022 Damian Ziolo");

if (!ConfigManager.Init())
{
    CConsole.WriteLine("Couldn't load or create configuration file", CConsoleType.Error);
    return;
}

if (ConfigManager.Properties.CheckUpdates)
{
    CConsole.WriteLine("Checking for updates");
    if (await UpdateService.CheckForUpdates())
    {
        string changelog = UpdateService.GetChangelog();
        CConsole.WriteLine("New update available", CConsoleType.Warn);
        CConsole.WriteLine
        (
            changelog,
            CConsoleType.Warn,
            writeToFile: false,
            trimMessage: false
        );
    }
    else
    {
        CConsole.WriteLine("No updates available");
    }
}


ConfigManager.Properties.EnsureDirsExist();

if (!FFmpegService.IsFFmpegInstalled())
{
    CConsole.WriteLine("FFmpeg is missing", CConsoleType.Error);
    return;
}

string input = string.Empty;

if (args.Length == 0)
{
    CConsole.WriteLine("No arguments provided. Awaiting for input", CConsoleType.Warn);
    input = CConsole.ReadLine();
}
else
{
    input = args[0];
}

if(input == string.Empty)
{
    CConsole.WriteLine("Input has to contain something", CConsoleType.Error);
    return;
}

CConsole.WriteLine("Input: " + input, CConsoleType.Debug);

CConsole.WriteLine("Logging to Spotify");
SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null) 
{
    CConsole.WriteLine("Couldn't sign in to Spotify. Exiting", CConsoleType.Error);
    return;
};

CConsole.WriteLine("Logged in successfully");

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
        CConsole.WriteLine("User request type: SingleTrack");
        CConsole.WriteLine("Queueing track...");
        await SpotifyService.PackSingleTrack(client, input, trackInfoBag, uriType);
        break;

    case SpotifyBrowseCategory.Playlist:
        CConsole.WriteLine("User request type: Playlist");
        CConsole.WriteLine("Queueing tracks... (It might take some time)");
        await SpotifyService.PackPlaylistTracks(client, input, trackInfoBag);
        break;

    case SpotifyBrowseCategory.Album:
        CConsole.WriteLine("User request type: Album");
        CConsole.WriteLine("Queueing tracks... (It might take some time)");
        await SpotifyService.PackAlbumTracks(client, input, trackInfoBag);
        break;
}

int workersCount = ConfigManager.Properties.WorkersCount;

if(workersCount < 1 || workersCount > 4)
{
    CConsole.WriteLine("WorkersCount has to be set in range of 1-4. Changing to 4", CConsoleType.Warn);
    workersCount = 4;
}

for (int i = 0; i < workersCount; i++)
{
    CConsole.WriteLine("Waiting for task...", writeToFile: false);
}

CConsole.WriteLine("Press CTRL-C to abort", writeToFile: false);

int topCursorPosition = Console.CursorTop - workersCount - 1;

await Task.WhenAll(Enumerable.Range(0, workersCount).Select(async workerId =>
{
    int positionY = topCursorPosition + workerId;
    while (trackInfoBag.TryTake(out TrackInfoModel? trackInfo))
    {
        string safeArtistName = FilenameResolver.RemoveForbiddenChars(trackInfo.Artist, StringType.Filename);
        string safeTitle = FilenameResolver.RemoveForbiddenChars(trackInfo.Title, StringType.Filename);
        string fullName = safeArtistName + " - " + safeTitle;

        DirectoryInfo trackDir = Directory.CreateDirectory
                (
                    Path.Combine
                    (
                        ConfigManager.Properties.MusicDirectory,
                        FilenameResolver.RemoveForbiddenChars(trackInfo.Playlist, StringType.Filename)
                    )
                );

        string convertedFilePath = Path.Combine(trackDir.FullName, fullName) + ".mp3";

        if (File.Exists(convertedFilePath))
        {
            CConsole.Overwrite($"W #{workerId} ::: Skipping: {fullName}", positionY);
            await Task.Delay(250);
            continue;
        }

        //CConsole.WriteLine($"W #{workerId} ::: Getting Lyrics ::: {fullName}", CConsoleType.Debug);
        /*Task<string> lyricsTask = 
            MusixmatchService.SearchLyricsFromText(fullName);*/

        CConsole.WriteLine($"W #{workerId} ::: Getting youtube links ::: {fullName}", CConsoleType.Debug);
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
                $"W #{workerId} ::: Could't retrieve audiostream: {fullName}",
                positionY,
                CConsoleType.Warn
            );
            await Task.Delay(1000);
            continue;
        }

        CConsole.WriteLine($"W #{workerId} ::: Downloading ::: {fullName}", CConsoleType.Debug);

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
                        "W #{0} ::: {1}{2} D: {3}% Q:{4} ::: {5}",
                        workerId,
                        new string('█', (int)(percentage / 5)),
                        new string('▓', 20 - (int)(percentage / 5)),
                        percentage.ToString("D3"),
                        trackInfoBag.Count.ToString("D3"),
                        fullName
                    ),
                    positionY,
                    writeToFile: false
                );
            }));
        
        CConsole.WriteLine($"W #{workerId} ::: Converting ::: {fullName}", CConsoleType.Debug);

        using (audioStream)
        {
            await FFmpegService.ConvertStreamAsync(audioStream, convertedFilePath,
                new Progress<Tuple<TimeSpan, TimeSpan>>
                (
                    pValues => 
                    {
                        int duration = (int)pValues.Item2.TotalSeconds;
                        int position = (int)pValues.Item1.TotalSeconds;
                        int percentage = 0;

                        if(duration > 0)
                        {
                            percentage = (int)Math.Ceiling(100.0 * position / duration);

                            CConsole.Overwrite
                            (
                                string.Format
                                (
                                    "W #{0} ::: {1}{2} C: {3}% Q:{4} ::: {5}",
                                    workerId,
                                    new string('█', (int)(percentage / 5)),
                                    new string('▓', 20 - (int)(percentage / 5)),
                                    percentage.ToString("D3"),
                                    trackInfoBag.Count.ToString("D3"),
                                    fullName
                                ),
                                positionY,
                                writeToFile: false
                            );
                        }
                    }
                )
            );
        }

        CConsole.WriteLine($"W #{workerId} ::: Adding metadata ::: {fullName}", CConsoleType.Debug);
        if (!File.Exists(convertedFilePath))
        {
            CConsole.WriteLine($"W #{workerId} ::: Not Exist In Folder ::: {fullName}", CConsoleType.Debug);
            continue;
        }

        using (TagLib.File file = TagLib.File.Create(convertedFilePath))
        {
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            file.Tag.Performers = new string[] { trackInfo.Artist };
            file.Tag.AlbumArtists = new string[] { trackInfo.Artist };
            file.Tag.Composers = new string[] { trackInfo.Artist };
            file.Tag.Copyright = trackInfo.Copyright;
            //file.Tag.Lyrics = await lyricsTask;
            file.Tag.Title = trackInfo.Title;
            file.Tag.Album = trackInfo.Album;
            file.Tag.Track = Convert.ToUInt32(trackInfo.TrackNumber);
            file.Tag.Disc = Convert.ToUInt32(trackInfo.DiscNumber);
            file.Tag.Year = Convert.ToUInt32(trackInfo.Year);
            file.Tag.Comment = trackInfo.Url;
            file.Tag.Genres = new string[] { trackInfo.Genres };
            if (trackInfo.AlbumPicture != string.Empty)
            {
                file.Tag.Pictures = new TagLib.Picture[]
                {
                    new TagLib.Picture(await albumPictureTask)
                };
            }
            file.Save();
        }

        CConsole.WriteLine($"W #{workerId} ::: Finished current task ::: {fullName}", CConsoleType.Debug);
    }
    CConsole.Overwrite($"W #{workerId} ::: Finished all tasks", positionY);
}));
