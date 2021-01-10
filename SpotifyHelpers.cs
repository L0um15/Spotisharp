using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
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
        public static async Task<TrackInfo> GetSpotifyTrack(this SpotifyClient client, string input)
        {
            FullTrack track;
            if (!input.IsSpotifyUrl())
            {
                var searchResult = await client.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
                var tracks = searchResult.Tracks;
                if (tracks.Items.Count == 0)
                {
                    Console.WriteLine("Spotify returned no results matching criteria.");
                    return null;
                }
                track = tracks.Items[0];
            }
            else
            {
                track = await client.Tracks.Get(input.GetSpotifyUrlId().Url);
            }
            var artist = await client.Artists.TryGet(track.Artists[0].Id);
            var album = await client.Albums.TryGet(track.Album.Id);
            string safeArtistName = artist.Name.MakeSafe();
            string safeTitle = track.Name.MakeSafe();
            int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
            if (WasDownloadedBefore(safeArtistName, safeTitle))
            {
                Console.WriteLine($"Skipping    ::::: {safeArtistName} - {safeTitle}");
                return null;
            }
            return new TrackInfo()
            {
                Artist = safeArtistName,
                Title = safeTitle,
                Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                Playlist = string.Empty,
                Album = album.Name,
                SpotifyUrl = track.ExternalUrls["spotify"],
                YoutubeUrl = GetYoutubeUrl($"{safeArtistName} {safeTitle}".MakeUriSafe()),
                Genres = artist.Genres.FirstOrDefault() != null ? artist.Genres[0] : "",
                AlbumArt = GetFileFromUrl(album.Images[0].Url),
                Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                Year = safeDate,
                DiscNumber = track.DiscNumber,
                TrackNumber = track.TrackNumber
            };
        }


        public static async Task QueueSpotifyTracksFromPlaylist(this SpotifyClient client, string url, ConcurrentQueue<TrackInfo> queue)
        {
            var playlist = await client.Playlists.Get(url);
            await foreach(var item in client.Paginate(playlist.Tracks))
            {
                if(item.Track is FullTrack track)
                {
                    var artist = await client.Artists.TryGet(track.Artists[0].Id);
                    var album = await client.Albums.TryGet(track.Album.Id);
                    string safeArtistName = artist.Name.MakeSafe();
                    string safeTitle = track.Name.MakeSafe();
                    int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
                    if (WasDownloadedBefore(safeArtistName, safeTitle))
                    {
                        Console.WriteLine($"Skipping    ::::: {safeArtistName} - {safeTitle}");
                        continue;
                    }
                    queue.Enqueue(new TrackInfo() {
                        Artist = safeArtistName,
                        Title = safeTitle,
                        Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                        Playlist = playlist.Name.MakeSafe(),
                        Album = album.Name,
                        SpotifyUrl = track.ExternalUrls["spotify"],
                        YoutubeUrl = GetYoutubeUrl($"{safeArtistName} {safeTitle}".MakeUriSafe()),
                        Genres = artist.Genres.FirstOrDefault() != null ? artist.Genres[0] : string.Empty,
                        AlbumArt = GetFileFromUrl(album.Images[0].Url),
                        Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                        Year = safeDate,
                        DiscNumber = track.DiscNumber,
                        TrackNumber = track.TrackNumber
                    });
                }
            }
        }

        public static async Task QueueSpotifyTracksFromAlbum(this SpotifyClient client, string url, ConcurrentQueue<TrackInfo> queue)
        {
            var album = await client.Albums.TryGet(url);
            var artist = await client.Artists.TryGet(album.Artists[0].Id);
            await foreach(var track in client.Paginate(album.Tracks))
            {
                
                string safeArtistName = artist.Name.MakeSafe();
                string safeTitle = track.Name.MakeSafe();
                int safeDate = DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
                if (WasDownloadedBefore(safeArtistName, safeTitle))
                {
                    Console.WriteLine($"Skipping    ::::: {safeArtistName} - {safeTitle}");
                    continue;
                }
                queue.Enqueue(new TrackInfo
                {
                    Artist = safeArtistName,
                    Title = safeTitle,
                    Lyrics = GetLyricsFromWeb($"{safeArtistName} {safeTitle}"),
                    Playlist = album.Name.MakeSafe(),
                    Album = album.Name,
                    SpotifyUrl = track.ExternalUrls["spotify"],
                    YoutubeUrl = GetYoutubeUrl($"{safeArtistName} {safeTitle}".MakeUriSafe()),
                    Genres = artist.Genres.FirstOrDefault() != null ? artist.Genres[0] : string.Empty,
                    AlbumArt = GetFileFromUrl(album.Images[0].Url),
                    Copyright = album.Copyrights.FirstOrDefault() != null ? album.Copyrights[0].Text : $"©{safeDate} {safeArtistName}",
                    Year = safeDate,
                    DiscNumber = track.DiscNumber,
                    TrackNumber = track.TrackNumber
                });
            }
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

        private static byte[] GetFileFromUrl(string url)
        {
            return new WebClient().DownloadData(url);
        }

        private static string GetYoutubeUrl(string fullName)
        {
            var searchFor = "https://www.youtube.com/results?search_query=" + fullName;
            var result = new WebClient().DownloadString(searchFor);
            var firstOccurance = Regex.Match(result, @"v=[a-zA-Z0-9_-]{11}");
            return "https://www.youtube.com/watch?" + firstOccurance.Value;
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
            
            return lyrics?.InnerText;
        }

        private static bool WasDownloadedBefore(string safeArtistName, string safeTitle)
        {
            var doesExist = Directory.GetFiles(Config.Properties.DownloadPath, "*.mp3", SearchOption.AllDirectories)
                .Any(x => x.Contains($"{safeArtistName} - {safeTitle}"));
            return doesExist;
        }
    }
    public class TrackInfo
    {
        public string Artist;
        public string Title;
        public string Lyrics;
        public string Playlist;
        public string Album;
        public string SpotifyUrl;
        public string YoutubeUrl;
        public string Genres;
        public byte[] AlbumArt;
        public string Copyright;
        public int TrackNumber;
        public int DiscNumber;
        public int Year;

        public override string ToString()
        {
            return $"{Artist} - {Title} from the album {Album}";
        }
    }
}