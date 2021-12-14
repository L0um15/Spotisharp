using SpotifyAPI.Web;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Spotisharp.Client.Services;

public static class SpotifyService
{
    public static async Task<TrackInfoModel?> GetSingleTrack(SpotifyClient client, string input, SpotifyUriType spotifyUriType = SpotifyUriType.None)
    {
        FullTrack? track = null;

        if(spotifyUriType == SpotifyUriType.None)
        {
            SearchResponse searchResponse = await client.Search.Item
                (
                    new SearchRequest
                        (
                            SearchRequest.Types.Track,
                            input
                        )
                );

            if(searchResponse.Tracks.Items != null)
            {
                track = searchResponse.Tracks.Items[0];  
            }
        }
        else
        {
            track = await client.Tracks.Get(input);
        }

        if(track != null)
        {
            FullAlbum album = await client.Albums.TryGet(track.Album.Id);
            return new TrackInfoModel()
            {
                Artist = track.Artists[0].Name,
                Title = track.Name,
                Url = track.ExternalUrls["spotify"],
                DiscNumber = track.DiscNumber,
                TrackNumber = track.TrackNumber,
                Album = album.Name,
                AlbumPicture = album.Images[0].Url,
                Copyright = album.Copyrights.FirstOrDefault()?.Text ?? string.Empty,
                Genres = album.Genres.FirstOrDefault() ?? string.Empty,
                Date = album.ReleaseDate
            };
        }
        return null;
    }

    public static async Task PackPlaylistTracks(SpotifyClient client, string input, ConcurrentBag<TrackInfoModel> bag)
    {
        FullPlaylist playlist = await client.Playlists.Get(input);
        if (playlist.Tracks != null)
        {
            await foreach(var item in client.Paginate(playlist.Tracks))
            {
                if (item.Track is FullTrack track)
                {
                    FullAlbum album = await client.Albums.TryGet(track.Album.Id);
                    Console.Write($"\rQueued: {bag.Count}");
                    bag.Add(new TrackInfoModel()
                    {
                        Artist = track.Artists[0].Name,
                        Title = track.Name,
                        Url = track.ExternalUrls["spotify"],
                        DiscNumber = track.DiscNumber,
                        TrackNumber = track.TrackNumber,
                        Album = album.Name,
                        AlbumPicture = album.Images[0].Url,
                        Copyright = album.Copyrights.FirstOrDefault()?.Text ?? string.Empty,
                        Genres = album.Genres.FirstOrDefault() ?? string.Empty,
                        Date = album.ReleaseDate
                    });
                }
            }
        }
    }

    public static async Task PackAlbumTracks(SpotifyClient client, string input, ConcurrentBag<TrackInfoModel> bag)
    {
        FullAlbum album = await client.Albums.Get(input);

        await foreach (SimpleTrack track in client.Paginate(album.Tracks))
        {
            bag.Add(new TrackInfoModel()
            {
                Artist = track.Artists[0].Name,
                Title = track.Name,
                Url = track.ExternalUrls["spotify"],
                DiscNumber = track.DiscNumber,
                TrackNumber = track.TrackNumber,
                Album = album.Name,
                AlbumPicture = album.Images[0].Url,
                Copyright = album.Copyrights.FirstOrDefault()?.Text ?? string.Empty,
                Genres = album.Genres.FirstOrDefault() ?? string.Empty,
                Date = album.ReleaseDate
            });
        }
    }

    private static async Task<FullAlbum> TryGet(this IAlbumsClient client, string albumid)
    {
        FullAlbum album;
        TryAgain:
            try
            {
                album = await client.Get(albumid);
                return album;
            }
            catch (APITooManyRequestsException ex)
            {
                await Task.Delay((int)(ex.RetryAfter.TotalMilliseconds));
                goto TryAgain;
            }
    }
}
