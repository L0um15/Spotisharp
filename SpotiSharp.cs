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
            if (args.Length != 1)
            {
                Console.WriteLine();
                Console.WriteLine("\tHelp Page: ");
                Console.WriteLine("\tUsage: .\\SpotiSharp \"<Search/SpotifyURL>\"");
                Console.WriteLine();
                return;
            };

            new VersionChecker().checkForUpdates();

            //Check if FFmpeg is installed.
            Console.WriteLine("Testing FFmpeg...");
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ffmpeg", "ffprobe");
            if (!File.Exists("ffmpeg.exe") || !File.Exists("ffprobe.exe"))
                Console.WriteLine("FFmpeg not installed. Downloading.");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            Console.WriteLine("FFmpeg was found.");

            string keyboardInput = args[0];

            if (IsValidUrl(keyboardInput))
            {
                if (keyboardInput.Contains("track"))
                {
                    await SearchProvider.SearchSpotifyByLink(keyboardInput, configuration);
                }
                else if (keyboardInput.Contains("playlist"))
                {
                    await SearchProvider.SearchSpotifyByPlaylist(keyboardInput, configuration);
                    return;
                }
            }
            else
                await SearchProvider.SearchSpotifyByText(keyboardInput, configuration);
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
