using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        public static int TrackNr { get; set; }
        public static string Album { get; set; }
        public static string Url { get; set; }
        public static int Year { get; set; }
        public static string Comments { get; set; }
        public static string Genres { get; set; }
        public static string AlbumArt { get; set; }
    }

    class SearchProvider
    {
        public static async Task SearchSpotifyByText(string input, ConfigurationHandler configuration) {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            SearchResponse searchResponse = null;
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            try
            {
                searchResponse = await spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
                var track = searchResponse.Tracks.Items[0];
                var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
                TrackInfo.Artist = track.Artists[0].Name;
                TrackInfo.Title = track.Name;
                TrackInfo.Album = track.Album.Name;
                TrackInfo.Genres = artist.Genres[0];
                TrackInfo.Url = track.ExternalUrls.First().Value;
                TrackInfo.TrackNr = track.TrackNumber;
                TrackInfo.Year = Convert.ToDateTime(track.Album.ReleaseDate).Year;
                TrackInfo.AlbumArt = track.Album.Images[0].Url;
                
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
        }

        public static async Task<string> SearchSpotifyByLink(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);

            var spotifyTrackID = Regex.Match(input, @"[^/]+$");

            var track = spotifyClient.Tracks.Get(spotifyTrackID.Value).Result;
            var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
            TrackInfo.Artist = track.Artists[0].Name;
            TrackInfo.Title = track.Name;
            TrackInfo.Album = track.Album.Name;
            TrackInfo.Genres = artist.Genres[0];
            TrackInfo.Url = track.ExternalUrls.First().Value;
            TrackInfo.TrackNr = track.TrackNumber;
            TrackInfo.Year = Convert.ToDateTime(track.Album.ReleaseDate).Year;
            TrackInfo.AlbumArt = track.Album.Images[0].Url;
            return $"{TrackInfo.Artist} - {TrackInfo.Title}";
        }

        public static async Task<string> SearchYoutubeByText(string input)
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
