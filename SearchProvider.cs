using HtmlAgilityPack;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
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
        static string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        static string myFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "SpotiSharp");
        public static async Task SearchSpotifyByText(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            var searchResponse = await spotifyClient.Search.Item(new SearchRequest(SearchRequest.Types.Track, input));
            var tracks = searchResponse.Tracks;
            if (tracks.Items.Count == 0)
            {
                Console.WriteLine("Spotify returned no results. Exiting.");
                Environment.Exit(0);
            }
            var item = tracks.Items[0];
            var album = await spotifyClient.Albums.Get(item.Album.Id);
            var artist = await spotifyClient.Artists.Get(item.Artists[0].Id);
            SetMetaData(item, artist, album);
            var fullName = $"{TrackInfo.Artist} - {TrackInfo.Title}";
            var doesExist = Directory.GetFiles(musicFolder, "*.mp3", SearchOption.AllDirectories)
                .Any(x => x.Contains(fullName));
            if (doesExist != true)
            {
                SearchMusixMatchByText(fullName);
                await DownloadHandler.DownloadTrack(SearchYoutubeByText(fullName), myFolder);
            }
            else
                Console.WriteLine("Track Found. Skipping");

        }
        public static async Task SearchSpotifyByLink(string input, ConfigurationHandler configuration)
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
            var fullName = $"{TrackInfo.Artist} - {TrackInfo.Title}";
            var doesExist = Directory.GetFiles(musicFolder, "*.mp3", SearchOption.AllDirectories)
                .Any(x => x.Contains(fullName));
            if (doesExist == false)
            {
                SearchMusixMatchByText(fullName);
                await DownloadHandler.DownloadTrack(SearchYoutubeByText(fullName), myFolder);
            }
            else
                Console.WriteLine("Track Found. Skipping");
        }

        public static async Task SearchSpotifyByAlbum(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            var spotifyAlbumID = Regex.Match(input, @"(?<=album\/)\w+");
            var album = await spotifyClient.Albums.Get(spotifyAlbumID.Value);
            int i = 1;
            foreach(var item in album.Tracks.Items)
            {
                var track = await spotifyClient.Tracks.Get(item.Id);
                var artist = await spotifyClient.Artists.Get(item.Artists[0].Id);
                SetMetaData(track, artist, album);
                var fullName = $"{TrackInfo.Artist} - {TrackInfo.Title}";
                Console.Clear();
                Console.WriteLine($"Downloading Track: {fullName} | {i}/{album.Tracks.Items.Count}\nInformation:\n\n");
                var doesExist = Directory.GetFiles(musicFolder, "*.mp3", SearchOption.AllDirectories)
                    .Any(x => x.Contains(fullName));
                if (doesExist == false)
                {
                    SearchMusixMatchByText(fullName);
                    await DownloadHandler.DownloadTrack(SearchYoutubeByText(fullName), myFolder);
                }
                else
                {
                    Console.WriteLine("Track Found. Skipping");
                    await Task.Delay(500);
                }
                i++;
            }
        }

        public static async Task SearchSpotifyByPlaylist(string input, ConfigurationHandler configuration)
        {
            var loginRequest = new ClientCredentialsRequest(configuration.CLIENTID, configuration.SECRETID);
            var loginResponse = await new OAuthClient().RequestToken(loginRequest);
            var spotifyClient = new SpotifyClient(loginResponse.AccessToken);
            var spotifyPlaylistID = Regex.Match(input, @"(?<=playlist\/)\w+");
            var playlist = await spotifyClient.Playlists.Get(spotifyPlaylistID.Value);
            int i = 1;
            foreach (var item in playlist.Tracks.Items)
            {
                if (item.Track is FullTrack track)
                {
                    var artist = await spotifyClient.Artists.Get(track.Artists[0].Id);
                    var album = await spotifyClient.Albums.Get(track.Album.Id);
                    SetMetaData(track, artist, album);
                    var fullName = $"{TrackInfo.Artist} - {TrackInfo.Title}";
                    Console.Clear();
                    Console.WriteLine($"Downloading Track: {fullName} | {i}/{playlist.Tracks.Items.Count}\nInformation:\n\n");
                    var doesExist = Directory.GetFiles(musicFolder, "*.mp3", SearchOption.AllDirectories)
                        .Any(x => x.Contains(fullName));
                    if (doesExist != true)
                    {
                        SearchMusixMatchByText(fullName);
                        await DownloadHandler.DownloadTrack(SearchYoutubeByText(fullName), myFolder);
                    }
                    else
                    {
                        Console.WriteLine("Track Found. Skipping");
                        await Task.Delay(500);
                    }
                    i++;
                }
            }
        }

        private static string SearchYoutubeByText(string input)
        {
            string youtubeSearchUrl = "https://www.youtube.com/search?q=";
            string formattedSearchQuery = Regex.Replace(input, @"[\s+\&]", "%20");
            var httpClient = new HttpClient();
            var htmlPage = httpClient.GetStringAsync(youtubeSearchUrl + formattedSearchQuery);
            List<string> matches = new List<string>();
            // Get all matches
            foreach (Match match in Regex.Matches(htmlPage.Result, @"v=[a-zA-Z0-9_-]{11}"))
                matches.Add(match.Value);
            // Get first element of list
            string youtubeTrackUrl = "https://youtube.com/watch?" + matches.First();
            return youtubeTrackUrl;
        }

        private static void SearchMusixMatchByText(string input)
        {

            // Scrap lyrics from Musixmatch.com

            string musixMatchMain = "https://www.musixmatch.com";
            string musixMatchSearch = "https://www.musixmatch.com/search/";
            var htmlWeb = new HtmlWeb();
            var htmlDoc = htmlWeb.Load(new Uri(musixMatchSearch + input));
            var node = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div/div/div/div[2]/div/div[1]/div[1]");
            if (node == null)
            {
                Console.WriteLine($"MusixMatch returned no results for: {input}");
                TrackInfo.Lyrics = null;
                return;
            }
            var link = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div/div[2]/div/div/div/div[2]/div/div[1]/div[1]/div[2]/div/ul/li/div/div[2]/div/h2/a").Attributes["href"].Value;
            htmlDoc = htmlWeb.Load(new Uri(musixMatchMain + link));
            var lyrics = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='mxm-lyrics']/span");
            // Scrap Unverified Lyrics if original was not found
            if (lyrics == null)
                lyrics = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__ok']");
            if (lyrics == null)
                lyrics = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__warning']");
            // No results? Return null and proceed
            if (lyrics == null)
            {
                Console.WriteLine($"MusixMatch returned no results for: {input}");
                TrackInfo.Lyrics = null;
            }
            else
            {
                TrackInfo.Lyrics = lyrics.InnerText;
                Console.WriteLine($"MusixMatch returned: {musixMatchMain + link}");
            }
        }

        private static void SetMetaData(FullTrack track, FullArtist artist, FullAlbum album)
        {
            TrackInfo.Title = Regex.Replace(track.Name, @"[\/\\\?\*\<\>\|\:\""]", " ");
            TrackInfo.Url = track.ExternalUrls.First().Value;
            TrackInfo.DiscNr = track.DiscNumber;
            TrackInfo.TrackNr = track.TrackNumber;
            TrackInfo.Artist = Regex.Replace(artist.Name, @"[\/\\\?\*\<\>\|\:\""]", " ");
            TrackInfo.AlbumArt = album.Images.First().Url;
            TrackInfo.Year =  DateTime.TryParse(album.ReleaseDate, out var value) ? value.Year : int.Parse(album.ReleaseDate);
            TrackInfo.Album = album.Name;
            // Sometimes Track has no Genres information. Return blank field.
            TrackInfo.Genres = artist.Genres.FirstOrDefault() != null ? artist.Genres.First() : "";
            // Sometimes Track has no copyright entry. Include Year and artist name instead of blank field.
            TrackInfo.Copyright = album.Copyrights.FirstOrDefault() != null
                ? album.Copyrights.First().Text : $"{TrackInfo.Year} {TrackInfo.Artist}";
        }
    }
}
