using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpotiSharp
{

    class TrackInfo
    {
        public static string Artist { get; set; }
        public static string Title { get; set; }
        public static string Album { get; set; }
        public static string TrackId { get; set; }
    }

    class SearchProvider
    {


        public static async Task SearchSpotify(string input, ConfigurationHandler configuration) {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            SearchResponse searchResponse = null;
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            try
            {
                searchResponse = await spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
                var track = searchResponse.Tracks.Items[0];
                TrackInfo.Artist = track.Artists[0].Name;
                TrackInfo.Title = track.Name;
                TrackInfo.Album = track.Album.Name;
                TrackInfo.TrackId = track.Id;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        public static async Task<string> SearchYoutube(string input)
        {
            string youtubeSearchUrl = "https://www.youtube.com/results?search_query=";
            string unFormatedSearchQuery = $"{TrackInfo.Artist} - {TrackInfo.Title}";
            string formattedSearchQuery = Regex.Replace(unFormatedSearchQuery, "\\s+", "%20");
            var httpClient = new HttpClient();
            var htmlPage = httpClient.GetStringAsync(youtubeSearchUrl + formattedSearchQuery);
            Match match = Regex.Match(htmlPage.Result, "v=[a-zA-Z0-9]{11}");
            string youtubeTrackUrl = "https://youtube.com/watch?" + match.Value;
            return youtubeTrackUrl;
        }
    }
}
