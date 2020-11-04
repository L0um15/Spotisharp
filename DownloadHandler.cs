using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;
using Xabe.FFmpeg;

namespace SpotiSharp
{
    class DownloadHandler
    {
        public static async Task DownloadTrack(string url, string destination)
        {
            if(!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            var youtubeEngine = YouTube.Default;
            var video = await youtubeEngine.GetVideoAsync(url);
            Console.WriteLine($"Downloading: {TrackInfo.Artist} - {TrackInfo.Title}");
            string trackName = $"{TrackInfo.Artist} - {TrackInfo.Title}" + video.FileExtension;
            File.WriteAllBytes(destination + trackName, video.GetBytes());
            Console.WriteLine("Converting to mp3");
            var mediaInfo = await FFmpeg.GetMediaInfo(destination + trackName);
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            IConversionResult conversionResult = await Conversion.New()
                .AddStream(audioStream)
                .SetOutput(destination + Path.GetFileNameWithoutExtension(trackName) + ".mp3")
                .SetOverwriteOutput(true)
                .Start();
            File.Delete(destination + trackName);
            Console.WriteLine("Done.");
        }
    }
}
