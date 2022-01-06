using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;
using System.Reflection;

DrawApplicationLogo();

CConsole.Note("Spotisharp v" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
CConsole.Note("\tCopyright \u00a92021 Damian Ziolo");

if (!FFmpegResolver.IsFFmpegInstalled())
{
    CConsole.Error("FFmpeg is missing.");
    return;
}

if (!ConfigManager.Init())
{
    CConsole.Error("Couldn't load or create configuration file.");
    return;
}

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

if (args.Length == 0)
{
    CConsole.Error("No arguments provided. Exiting.");
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
        CConsole.Note(string.Join("", ansiColoredChars));
    }
}