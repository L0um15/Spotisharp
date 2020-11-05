using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace SpotiSharp
{

    

    class SpotiSharp
    {
        ConfigurationHandler configuration = new ConfigurationHandler();
        public async Task MainAsync(string[] args)
        {
            // Print Help page when no arguments passed.
            if (args.Length != 1) {
                Console.WriteLine();
                Console.WriteLine("\tHelp Page: ");
                Console.WriteLine("\tUsage: .\\SpotiSharp \"<Search/SpotifyURL>\"");
                Console.WriteLine();
                return;
            };

            //Check for updates
            new VersionChecker().checkForUpdates();

            //Check if FFmpeg is installed.
            Console.WriteLine("Checking ffmpeg.");
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ffmpeg", "ffprobe");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            
            string url = null;
            string keyboardInput = args[0];
            if (IsValidUrl(keyboardInput))
            {
                // Search Spotify for specified Track bu URL.
                var text = await SearchProvider.SearchSpotifyByLink(keyboardInput, configuration);
                //Search for this track on Youtube.
                url = await SearchProvider.SearchYoutubeByText(text);
            }
            else
            {
                // Search Spotify for results, Exit when nothing were found.
                await SearchProvider.SearchSpotifyByText(keyboardInput, configuration);
                //Search for this track on Youtube.
                url = await SearchProvider.SearchYoutubeByText(keyboardInput);
            }
            
            // Print Found Track
            Console.WriteLine($"Spotify Returned: {TrackInfo.Artist} - {TrackInfo.Title}");
            
            //Try to download video and convert it to mp3.
            await DownloadHandler.DownloadTrack(url, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\SpotiSharp\\");
        }


        private bool IsValidUrl(string input)
        {
            if (Regex.IsMatch(input, @"([--:\w?@%&+~#=]*\.[a-z]{2,4}\/{0,2})((?:[?&](?:\w+)=(?:\w+))+|[--:\w?@%&+~#=]+)?"))
            {
                if (input.Contains("open.spotify.com"))
                    return true;
            }
            return false;
        }


    }
}
