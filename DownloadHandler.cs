using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VideoLibrary;
using Xabe.FFmpeg;

namespace SpotiSharp
{
    class DownloadHandler
    {
        public static async Task DownloadTrack(string url, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            var youtubeEngine = YouTube.Default;
            var video = await youtubeEngine.GetVideoAsync(url);
            Console.WriteLine($"Downloading: {TrackInfo.Artist} - {TrackInfo.Title}");
            string trackName = $"{TrackInfo.Artist} - {TrackInfo.Title}" + video.FileExtension;
            File.WriteAllBytes(destination + trackName, video.GetBytes());
            Console.WriteLine("Converting to mp3");
            var mediaInfo = await FFmpeg.GetMediaInfo(destination + trackName);
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            IConversionResult conversionResult = await FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .SetOutput(destination + Path.GetFileNameWithoutExtension(trackName) + ".mp3")
                .SetOverwriteOutput(true)
                .Start();
            File.Delete(destination + trackName);
            Console.WriteLine("Merging Metadata.");
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            TagLib.File file = TagLib.File.Create(destination + Path.GetFileNameWithoutExtension(trackName) + ".mp3");
            file.Tag.AlbumArtists = new string[] { TrackInfo.Artist };
            file.Tag.Performers = new string[] { TrackInfo.Artist };
            file.Tag.Composers = new string[] { TrackInfo.Artist };
            file.Tag.Copyright = TrackInfo.Copyright;
            file.Tag.Lyrics = TrackInfo.Lyrics;
            file.Tag.Title = TrackInfo.Title;
            file.Tag.Album = TrackInfo.Album;
            file.Tag.Track = Convert.ToUInt32(TrackInfo.TrackNr);
            file.Tag.Disc = Convert.ToUInt32(TrackInfo.DiscNr);
            file.Tag.Year = Convert.ToUInt32(TrackInfo.Year);
            file.Tag.Comment = TrackInfo.Url;
            file.Tag.Genres = new string[] { TrackInfo.Genres };
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(TrackInfo.AlbumArt);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            file.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(bytes) };
            file.Save();
            Console.WriteLine("Done.");
        }
    }
}
