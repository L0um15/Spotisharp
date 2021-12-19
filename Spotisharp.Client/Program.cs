using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;
using System.Reflection;

Console.WriteLine(@"
               ▓        ▓
               ▓        ▓      ▄▄
         ▄▄▄▓▓▓▓███▓▓▓▓▓▓▄▄▓▓▀▀▀
        ████████████████▓█████▄▄
            ▄▄▄▓▀▀▀     ▓ ▀▀█████
       ▄▓▓████████████▓▓▓▄▄
         ▀█▀▀▀▀▓▀▀▀▀▀▀▀██████▄
             ▄▄▓▄▄▄▄▄   ▓  ▀▀█▀
          █████▓█▀██████▓▓▄
               ▓        ▓▀██
               ▓        ▓
               ▓        ▓
               ▀        ▀                     
    ");

Console.WriteLine("Spotisharp v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + 
    "\n\tCopyright \u00a92021 Damian Ziolo\n");

if (!FFmpegResolver.IsFFmpegInstalled())
{
    Console.WriteLine("Error: FFmpeg is missing.");
    return;
}

if (!ConfigManager.Init())
{
    Console.WriteLine("Error: Couldn't load or create configuration file.");
    return;
}

if (ConfigManager.Properties.IsFirstTime)
{
    Console.WriteLine("Hi!, Thank you for using Spotisharp!\n" +
        "This message will be displayed only once\n" +
        "Spotisharp v3 is now using PKCETokenAuthentication which is linked to your personal Spotify account\n" +
        "For safety reasons please make sure you obtained this software from original repository\n" +
        "Spotisharp is free and opensource. So go ahead and read its source code\n" +
        "If you have any questions, go ahead and ask me on github\n" +
        "Github: https://github.com/L0um15/Spotisharp \n");
    ConfigManager.Properties.IsFirstTime = false;
    ConfigManager.WriteChanges();
}

if (args.Length == 0)
{
    Console.WriteLine("No arguments provided. Exiting.");
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