using System;
using System.IO;
using System.Reflection;
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
            if (args.Length != 1) return;

            //Check if FFmpeg is installed.
            Console.WriteLine("Checking ffmpeg.");
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ffmpeg", "ffprobe");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

            string keyboardInput = args[0];
            // Search Spotify for results, Exit when nothing were found.
            await SearchProvider.SearchSpotify(keyboardInput, configuration);
            // Print Found Track
            Console.WriteLine($"Spotify Returned: {TrackInfo.Artist} - {TrackInfo.Title}");
            //Search for this track on Youtube.
            var url = await SearchProvider.SearchYoutube(keyboardInput);
            //Try to download video and convert it to mp3.
            await DownloadHandler.DownloadTrack(url, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\SpotiSharp\\");
        }

    }
}
