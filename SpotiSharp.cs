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
                Console.WriteLine("\n\tHelp Page: \n" +
                    "\tUsage: .\\SpotiSharp \"<Search/SpotifyURL>\"");
                return;
            };

            new VersionChecker().checkForUpdates();

            //Check if FFmpeg is installed.
            Console.WriteLine("Looking for FFmpeg.\n" +
                "Please be patient.");
            FFmpeg.SetExecutablesPath(Directory.GetCurrentDirectory(), "ffmpeg", "ffprobe");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            string keyboardInput = args[0];

            if (IsValidUrl(keyboardInput))
            {
                switch (keyboardInput)
                {
                    case "track":
                        await SearchProvider.SearchSpotifyByLink(keyboardInput, configuration);
                        break;
                    case "playlist":
                        await SearchProvider.SearchSpotifyByPlaylist(keyboardInput, configuration);
                        break;
                    default:
                        Console.WriteLine("Sorry but this link format is not currently supported.\n" +
                            "You can always request a new feature on SpotiSharp Github Issue Tracker");
                        break;
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
