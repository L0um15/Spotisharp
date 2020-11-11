using HtmlAgilityPack;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpotiSharp
{

    class TrackInfo
    {
        public static string Artist { get; set; }
        public static string Title { get; set; }
        public static string Lyrics { get; set; }
        public static int TrackNr { get; set; }
        public static int DiscNr { get; set; }
        public static string Album { get; set; }
        public static string Url { get; set; }
        public static int Year { get; set; }
        public static string Comments { get; set; }
        public static string Genres { get; set; }
        public static string AlbumArt { get; set; }
        public static string Copyright { get; set; }
    }

    class SearchProvider
    {
        public static async Task<string> SearchSpotifyByText(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            SearchResponse searchResponse = null;
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            searchResponse = await spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
            var track = searchResponse.Tracks.Items[0];
            if(track == null)
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
            var album = await spotifyClient.Albums.Get(track.Album.Id);
            var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
            SetMetaData(track, artist, album);
            return $"{TrackInfo.Artist} - {TrackInfo.Title}";
        }
        public static async Task<string> SearchSpotifyByLink(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            var spotifyTrackID = Regex.Match(input, @"(?<=track\/)\w+");
            var track = await spotifyClient.Tracks.Get(spotifyTrackID.Value);
            if (track == null) 
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
            var album = await spotifyClient.Albums.Get(track.Album.Id);
            var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
            SetMetaData(track, artist, album);
            return $"{TrackInfo.Artist} - {TrackInfo.Title}";
        }

        public static string SearchYoutubeByText(string input)
        {
            string youtubeSearchUrl = "https://www.youtube.com/results?search_query=";
            string formattedSearchQuery = Regex.Replace(input, "\\s+", "%20");
            var httpClient = new HttpClient();
            var htmlPage = httpClient.GetStringAsync(youtubeSearchUrl + formattedSearchQuery);
            // Gets first vidId from html code. 
            Match match = Regex.Match(htmlPage.Result, "v=[a-zA-Z0-9]{11}");
            string youtubeTrackUrl = "https://youtube.com/watch?" + match.Value;
            return youtubeTrackUrl;
        }

        public static void SearchMusixMatchByText(string input)
        {

            // Scrap lyrics from Musixmatch.com

            string musixMatchMain = "https://www.musixmatch.com";
            string musixMatchSearch = "https://www.musixmatch.com/search/";
            var htmlWeb = new HtmlWeb();
            var htmlDoc = htmlWeb.Load(new Uri(musixMatchSearch + input));
            var node = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div/div/div/div[2]/div/div[1]/div[1]/div[2]/div/ul/li/div/div[2]/div/h2/a").Attributes["href"].Value;
            htmlDoc = htmlWeb.Load(new Uri(musixMatchMain + node));
            var lyrics = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/div/main/div/div/div[3]/div[1]/div/div/div/div[2]/div[1]/span");
            // Scrap Unverified Lyrics if original was not found
            if (lyrics == null)
                lyrics = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div/div/main/div/div/div[3]/div[1]/div/div/div/div[2]/div[2]/span");
            // No results? Return null and proceed
            if (lyrics == null)
            {
                Console.WriteLine($"MusixMatch returned no results for: {input}");
                TrackInfo.Lyrics = null;
            }
            else 
            {
                TrackInfo.Lyrics = lyrics.InnerText;
                Console.WriteLine($"MusixMatch returned: {musixMatchMain + node}");
            }
        }


        private static void SetMetaData(FullTrack track, FullArtist artist, FullAlbum album)
        {
            TrackInfo.Title = track.Name;
            TrackInfo.Url = track.ExternalUrls.First().Value;
            TrackInfo.DiscNr = track.DiscNumber;
            TrackInfo.TrackNr = track.TrackNumber;
            TrackInfo.Artist = artist.Name;
            TrackInfo.AlbumArt = album.Images.First().Url;
            TrackInfo.Year = Convert.ToDateTime(album.ReleaseDate).Year;
            TrackInfo.Album = album.Name;
            // Sometimes Track has no Genres information. Return blank field.
            TrackInfo.Genres = artist.Genres.First() != null ? artist.Genres.First() : "";
            // Sometimes Track has no copyright entry. Include Year and artist name instead of blank field.
            TrackInfo.Copyright = album.Copyrights.FirstOrDefault() != null 
                ? album.Copyrights.First().Text : $"{TrackInfo.Year} {TrackInfo.Artist}";
        }
    }
}
