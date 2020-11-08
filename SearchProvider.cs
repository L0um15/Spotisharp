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
        public static async Task<string> SearchSpotifyByText(string input, ConfigurationHandler configuration) {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            SearchResponse searchResponse = null;
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            try
            {
                searchResponse = await spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
                var track = searchResponse.Tracks.Items[0];
                var album = await spotifyClient.Albums.Get(track.Album.Id);
                var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
                await SetMetaData(track, artist, album);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
            return $"{TrackInfo.Artist} - {TrackInfo.Title}";
        }

        public static async Task<string> SearchSpotifyByLink(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            var spotifyTrackID = Regex.Match(input, @"(?<=track\/)\w+");
            try
            {
                var track = spotifyClient.Tracks.Get(spotifyTrackID.Value).Result;
                var album = await spotifyClient.Albums.Get(track.Album.Id);
                var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
                await SetMetaData(track, artist, album);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Sometimes Spotify wont return anything with direct link. Working on it.
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
            //Returns text that will be passed to youtube search.
            return $"{TrackInfo.Artist} - {TrackInfo.Title}";
        }

        public static async Task<string> SearchYoutubeByText(string input)
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

        private static async Task SetMetaData(FullTrack track, FullArtist artist, FullAlbum album)
        {
            TrackInfo.Title = track.Name;
            TrackInfo.Url = track.ExternalUrls.First().Value;
            TrackInfo.DiscNr = track.DiscNumber;
            TrackInfo.TrackNr = track.TrackNumber;
            TrackInfo.Artist = artist.Name;
            TrackInfo.Genres = artist.Genres[0];
            TrackInfo.Copyright = album.Copyrights[0].Text;
            TrackInfo.AlbumArt = album.Images[0].Url;
            TrackInfo.Year = Convert.ToDateTime(album.ReleaseDate).Year;
            TrackInfo.Album = album.Name;
        }
    }
}
