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
                Playlist = "Unknown",
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
            int i = 1;
            await foreach(var item in client.Paginate(playlist.Tracks))
            {
                if (item.Track is FullTrack track)
                {
                    FullAlbum album = await client.Albums.TryGet(track.Album.Id);
                    TrackInfoModel trackInfo = new TrackInfoModel()
                    {
                        Artist = track.Artists[0].Name,
                        Title = track.Name,
                        Url = track.ExternalUrls["spotify"],
                        Playlist = playlist.Name ?? string.Empty,
                        DiscNumber = track.DiscNumber,
                        TrackNumber = track.TrackNumber,
                        Album = album.Name,
                        AlbumPicture = album.Images[0].Url,
                        Copyright = album.Copyrights.FirstOrDefault()?.Text ?? string.Empty,
                        Genres = album.Genres.FirstOrDefault() ?? string.Empty,
                        Date = album.ReleaseDate
                    };
                    bag.Add(trackInfo);
                    CConsole.Debug($"Q: {i++.ToString("D3")} | Added: {trackInfo}");
                }
            }
        }
    }

    public static async Task PackAlbumTracks(SpotifyClient client, string input, ConcurrentBag<TrackInfoModel> bag)
    {
        int topCursorPosition = Console.CursorTop;
        FullAlbum album = await client.Albums.Get(input);
        int i = 1;
        await foreach (SimpleTrack track in client.Paginate(album.Tracks))
        {
            TrackInfoModel trackInfo = new TrackInfoModel()
            {
                Artist = track.Artists[0].Name,
                Title = track.Name,
                Url = track.ExternalUrls["spotify"],
                Playlist = album.Name ?? string.Empty,
                DiscNumber = track.DiscNumber,
                TrackNumber = track.TrackNumber,
                Album = album.Name ?? string.Empty,
                AlbumPicture = album.Images[0].Url,
                Copyright = album.Copyrights.FirstOrDefault()?.Text ?? string.Empty,
                Genres = album.Genres.FirstOrDefault() ?? string.Empty,
                Date = album.ReleaseDate
            };
            bag.Add(trackInfo);
            CConsole.Debug($"Q: {i++.ToString("D3")} | Added: {trackInfo}");
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
