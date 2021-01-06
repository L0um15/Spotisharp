using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpotiSharp
{
    public static class SpotifyHelpers
    {
        public static SpotifyClient ConnectToSpotify()
        {
            var loginRequest = new ClientCredentialsRequest(Config.Properties.ClientID, Config.Properties.ClientSecret);
            var loginResponse = new OAuthClient().RequestToken(loginRequest).GetAwaiter().GetResult();
            return new SpotifyClient(loginResponse.AccessToken);
        }
        public static async Task<(FullTrack track, FullAlbum album, FullArtist artist)> GetSpotifyTrackFromName(this SpotifyClient client, string input)
        {            
            var searchResult = client.Search.Item(new SearchRequest(SearchRequest.Types.Track, input)).GetAwaiter().GetResult();
            var tracks = searchResult.Tracks;
            if (tracks.Items.Count == 0)
            {
                Console.WriteLine("Spotify returned no results matching criteria.");
                Environment.Exit(1);
            }
            var track = tracks.Items[0];

            //var album = client.Albums.TryGet(track.Album.Id).GetAwaiter().GetResult();
            var artist = client.Artists.TryGet(track.Artists[0].Id).GetAwaiter().GetResult();
            
            return (tracks.Items[0], album, artist);
        }

        private static async Task<FullAlbum> TryGet(this IAlbumsClient client, string albumId)
        {
            FullAlbum album;
        TryAgain:
            try
            {
                album = client.Get(albumId);
            }
            catch (APITooManyRequestsException exception)
            {
                Console.WriteLine("Too many requests, throttling...");
                Task.Delay((int)(exception.RetryAfter.TotalMilliseconds) + 1000).GetAwaiter().GetResult();
                goto TryAgain;
            }

            return album;
        }
        private static FullArtist TryGet(this IArtistsClient client, string artistId)
        {
            FullArtist artist;
        TryAgain:
            try
            {
                artist = client.Get(artistId).GetAwaiter().GetResult();
            }
            catch (APITooManyRequestsException exception)
            {
                Console.WriteLine("Too many requests, throttling...");
                Task.Delay((int)(exception.RetryAfter.TotalMilliseconds) + 1000).GetAwaiter().GetResult();
                goto TryAgain;
            }

            return artist;
        }
    }


    public class TrackInfo
    {
        public string Artist;
        public string Title;
        public string Lyrics;
        public string Album;
        public string Url;
        public string Comments;
        public string Genres;
        public string AlbumArt;
        public string Copyright;
        public int TrackNumber;
        public int DiscNumber;
        public int Year;
    }
}