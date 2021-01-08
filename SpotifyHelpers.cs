using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
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
        public static TrackInfo GetSpotifyTrackFromName(this SpotifyClient client, string input)
        {            
            var searchResult = client.Search.Item(new SearchRequest(SearchRequest.Types.Track, input)).GetAwaiter().GetResult();
            var tracks = searchResult.Tracks;

            if (tracks.Items.Count == 0)
            {
                Console.WriteLine("Spotify returned no results matching criteria.");
                Environment.Exit(1);
            }
            var track = tracks.Items[0];

            var artistTask = client.Artists.TryGet(track.Artists[0].Id);

            var albumTask = client.Albums.TryGet(track.Album.Id);

            artistTask.Start();
            albumTask.Start();

            var (artist, album) = (artistTask.Result, albumTask.Result);

            string safeArtistName = artist.Name.MakeSafe();
            string safeTitle = track.Name.MakeSafe();
            int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
            return new TrackInfo()
            {
                Artist = safeArtistName,
                Title = safeTitle,
                Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                Album = album.Name,
                Url = track.ExternalUrls["spotify"],
                Genres = album.Genres.FirstOrDefault() != null ? album.Genres[0] : "",
                AlbumArt = album.Images[0].Url,
                Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                Year = safeDate,
                DiscNumber = track.DiscNumber,
                TrackNumber = track.TrackNumber

            };
        }


        public static async Task<TrackInfo[]> GetSpotifyTracksFromPlaylist(this SpotifyClient client, string url)
        {
            var playlist = await client.Playlists.Get(url);
            TrackInfo[] trackInfos = new TrackInfo[playlist.Tracks.Total.GetValueOrDefault()];
            int i = 0;
            await foreach(var item in client.Paginate(playlist.Tracks))
            {
                if(item.Track is FullTrack track)
                {
                    var artist = await client.Artists.TryGet(track.Artists[0].Id);
                    var album = await client.Albums.TryGet(track.Album.Id);
                    string safeArtistName = artist.Name.MakeSafe();
                    string safeTitle = track.Name.MakeSafe();
                    int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
                    trackInfos[i] = new TrackInfo()
                    {
                        Artist = safeArtistName,
                        Title = safeTitle,
                        Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                        Album = album.Name,
                        Url = track.ExternalUrls["spotify"],
                        Genres = album.Genres.FirstOrDefault() != null ? album.Genres[0] : "",
                        AlbumArt = album.Images[0].Url,
                        Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                        Year = safeDate,
                        DiscNumber = track.DiscNumber,
                        TrackNumber = track.TrackNumber
                    };
                }
                i++;
            }
            return trackInfos;
        }

        public static async Task<TrackInfo[]> GetSpotifyTrackFromAlbum(this SpotifyClient client, string url)
        {
            var album = await client.Albums.Get(url);
            TrackInfo[] trackInfos = new TrackInfo[album.Tracks.Total.GetValueOrDefault()];
            int i = 0;
            await foreach(var track in client.Paginate(album.Tracks))
            {
                var artist = await client.Artists.TryGet(album.Artists[0].Id);
                string safeArtistName = artist.Name.MakeSafe();
                string safeTitle = track.Name.MakeSafe();
                int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
                trackInfos[i] = new TrackInfo
                {
                    Artist = safeArtistName,
                    Title = safeTitle,
                    Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                    Album = album.Name,
                    Url = track.ExternalUrls["spotify"],
                    Genres = album.Genres.FirstOrDefault() != null ? album.Genres[0] : "",
                    AlbumArt = album.Images[0].Url,
                    Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                    Year = safeDate,
                    DiscNumber = track.DiscNumber,
                    TrackNumber = track.TrackNumber
                };
                i++;
            }
            return trackInfos;
        }


        private static async Task<FullAlbum> TryGet(this IAlbumsClient client, string albumId)
        {
            FullAlbum album;
        TryAgain:
            try
            {
                album = await client.Get(albumId);
            }
            catch (APITooManyRequestsException exception)
            {
                Console.WriteLine("Too many requests, throttling...");
                await Task.Delay((int)(exception.RetryAfter.TotalMilliseconds) + 1000);
                goto TryAgain;
            }

            return album;
        }
        private static async Task<FullArtist> TryGet(this IArtistsClient client, string artistId)
        {
            FullArtist artist;
        TryAgain:
            try
            {
                artist = await client.Get(artistId);
            }
            catch (APITooManyRequestsException exception)
            {
                Console.WriteLine("Too many requests, throttling...");
                await Task.Delay((int)(exception.RetryAfter.TotalMilliseconds) + 1000);
                goto TryAgain;
            }

            return artist;
        }

        private static string GetLyricsFromWeb(string fullName)
        {
            var searchFor = "https://www.musixmatch.com/search/" + fullName;
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(searchFor);
            var Node = htmlDocument.DocumentNode.SelectSingleNode("//ul[contains(@class, 'tracks') and contains(@class, 'list')]");
            if (Node == null) return null;
            var card = Node.SelectSingleNode("//a[@class='title'][1]"); // Return first occurance
            if (card == null) return null;
            string fullUrl = "https://www.musixmatch.com" + card.Attributes["href"].Value;
            htmlDocument = htmlWeb.Load(fullUrl);
            var lyrics = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='mxm-lyrics']/span"); // Approved Lyrics
            if (lyrics == null)
                lyrics = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__ok']"); // Correct Lyrics waiting for approval
            if (lyrics == null)
                lyrics = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__warning']"); // Incorrect Lyrics waiting for review.
            return lyrics != null ? lyrics.InnerText : null;
        }
    }

    public class TrackInfo
    {
        public string Artist;
        public string Title;
        public string Lyrics;
        public string Album;
        public string Url;
        public string Genres;
        public string AlbumArt;
        public string Copyright;
        public int TrackNumber;
        public int DiscNumber;
        public int Year;
    }
}