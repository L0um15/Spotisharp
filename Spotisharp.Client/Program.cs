using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;
using System.Reflection;

DrawApplicationLogo();

Logger.Note("Spotisharp v" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
Logger.Note("\tCopyright \u00a92021 Damian Ziolo");

if (!FFmpegResolver.IsFFmpegInstalled())
{
    Logger.Error("FFmpeg is missing.");
    return;
}

if (!ConfigManager.Init())
{
    Logger.Error("Couldn't load or create configuration file.");
    return;
}

if (ConfigManager.Properties.IsFirstTime)
{
    Logger.Note("Hey!, this is your first time eh? Lemme tell ya somethin important");
    Logger.Warn("Spotisharp is free and opensource software and it always will be");
    Logger.Warn("I'm treating this project as a hobby not as a duty");
    Logger.Warn("Stuff might get broken, some things might not work at all");
    Logger.Warn("Also make sure you're using official software");
    Logger.Note("Any collaboration will be appreciated. This includes features, bugfixes etc.");
    Logger.Note("Okay enough for now, lets get to work");
    ConfigManager.Properties.IsFirstTime = false;
    ConfigManager.WriteChanges();
}

if (args.Length == 0)
{
    Logger.Error("No arguments provided. Exiting.");
    return;
}

SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null) return;

string? input = args[0];

if(input == null) return;

SpotifyUriType uriType = SpotifyUriResolver.GetUriType(input);
SpotifyBrowseCategory category = SpotifyUriResolver.GetBrowseCategory(input);
ConcurrentBag<TrackInfoModel> bag = new ConcurrentBag<TrackInfoModel>();
string queryID = SpotifyUriResolver.GetID(input, uriType);

if (queryID != string.Empty)
{
    input = queryID;
}

switch (category)
{
    case SpotifyBrowseCategory.None:
    case SpotifyBrowseCategory.Track:
        TrackInfoModel? trackInfo = await SpotifyService.GetSingleTrack(client, input, uriType);
        if (trackInfo == null) return;
        Console.WriteLine(trackInfo);
        return;

    case SpotifyBrowseCategory.Playlist:
        await SpotifyService.PackPlaylistTracks(client, input, bag);
        break;

    case SpotifyBrowseCategory.Album:
        await SpotifyService.PackAlbumTracks(client, input, bag);
        break;
}

/*var pOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
await Parallel.ForEachAsync(bag, pOptions, async (track, cancellationToken) =>
{
    // Do something here
});*/

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
        Logger.Note(string.Join("", ansiColoredChars));
    }
}