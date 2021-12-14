using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;
using System.Collections.Concurrent;

if (ConfigManager.Init())
{
    Console.WriteLine("Loaded config file!");
}

SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null) return;

string? input = Console.ReadLine();

if(input == null) return;

if (SpotifyUriResolver.IsUriValid(input))
{
    SpotifyUriType uriType = SpotifyUriResolver.GetUriType(input);
    SpotifyBrowseCategory category = SpotifyUriResolver.GetBrowseCategory(input);
    ConcurrentBag<TrackInfoModel> bag = new ConcurrentBag<TrackInfoModel>();
    string queryID = SpotifyUriResolver.GetID(input, uriType);

    if(category == SpotifyBrowseCategory.Track)
    {
        TrackInfoModel? trackInfo = await SpotifyService.GetSingleTrack(client, queryID, uriType);
        if(trackInfo == null) return;
        // Do something here
        return;
    }

    if (category == SpotifyBrowseCategory.Playlist)
    {
        await SpotifyService.PackPlaylistTracks(client, queryID, bag);
    };
    if (category == SpotifyBrowseCategory.Album)
    {
        await SpotifyService.PackAlbumTracks(client, queryID, bag);
    }

    /*var pOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
    await Parallel.ForEachAsync(bag, pOptions, async (track, cancellationToken) =>
    {
        // Do something here
    });*/

}