using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace SpotiSharp
{
    static class SpotiSharp
    {
        public static async Task MainAsync(string[] args)
        {
            if (IsRoot)
            {
                Console.WriteLine("SpotiSharp does not require root privileges to run.\n" +
                    "Please try without sudo");
                Environment.Exit(0);
            }

            Config.Initialize(); // Initializes Configuration

            VersionChecker.checkForUpdates();

            // Print Help page when no arguments passed.
            if (args.Length != 1)
            {
                Console.WriteLine("\n\tHelp Page: \n" +
                    "\tUsage: .\\SpotiSharp \"<Search/SpotifyURL>\"\n");
                return;
            };

            //Check if FFmpeg is installed.
            Console.WriteLine("Looking for FFmpeg.\n" +
                "Please be patient.");
            FFmpeg.SetExecutablesPath(Config.FFmpegPath, "ffmpeg", "ffprobe");
            // Download latest version of ffmpeg to specified directory. Handle auto-updating
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, Config.FFmpegPath, new Progress<ProgressInfo>(progress =>
            {
                // Blank spaces are necessary to earse everything.
                Console.Write($"\rDownloaded Bytes: {progress.DownloadedBytes} of {progress.TotalBytes} | Downloading Latest FFmpeg                    ");
            }));
            Console.WriteLine();
            // Set execution permission for downloaded ffmpeg, ffprobe package.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
                Process.Start("chmod", $"+x {Path.Combine(Config.FFmpegPath, "ffmpeg")} {Path.Combine(Config.FFmpegPath, "ffprobe")}");

            string keyboardInput = args[0];

            if (IsValidUrl(keyboardInput))
            {
                if (keyboardInput.Contains("track"))
                    await SearchProvider.SearchSpotifyByLink(keyboardInput);
                else if (keyboardInput.Contains("playlist"))
                    await SearchProvider.SearchSpotifyByPlaylist(keyboardInput);
                else if (keyboardInput.Contains("album"))
                    await SearchProvider.SearchSpotifyByAlbum(keyboardInput);
                else
                    Console.WriteLine("Sorry but this link format is not currently supported.\n" +
                            "You can always request a new feature on SpotiSharp Github Issue Tracker");
            }
            else
                await SearchProvider.SearchSpotifyByText(keyboardInput);
        }
        private static bool IsValidUrl(string input)
        {
            if (Regex.IsMatch(input, @"([--:\w?@%&+~#=]*\.[a-z]{2,4}\/{0,2})((?:[?&](?:\w+)=(?:\w+))+|[--:\w?@%&+~#=]+)?"))
            {
                if (input.Contains("open.spotify.com"))
                    return true;
            }
            return false;
        }
        // Sudo makes you run program as root, just simply check if sudo was issued by checking username.
        private static bool IsRoot
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.UserName == "root" : false;
    }
}
