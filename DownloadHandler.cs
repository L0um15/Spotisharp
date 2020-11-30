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

        private static string UNCONVERTEDFILEPATH { get; set; }
        private static string CONVERTEDFILEPATH { get; set; }

        public static async Task DownloadTrack(string url, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            var youtubeEngine = YouTube.Default;
            var video = await youtubeEngine.GetVideoAsync(url);
            Console.WriteLine($"Status: {TrackInfo.Artist} - {TrackInfo.Title}");
            string trackName = $"{TrackInfo.Artist} - {TrackInfo.Title}" + video.FileExtension;
            UNCONVERTEDFILEPATH = Path.Combine(destination, trackName);
            CONVERTEDFILEPATH = Path.Combine(destination, Path.GetFileNameWithoutExtension(trackName) + ".mp3");
            // Disposes BinaryWriter after job is done.
            using (var writer = new BinaryWriter(File.Open(UNCONVERTEDFILEPATH, FileMode.OpenOrCreate)))
            {
                byte[] bytes = video.GetBytes();
                var bytesLeft = bytes.Length;
                var bytesWritten = 0;
                while (bytesLeft > 0)
                {
                    int chunk = Math.Min(1024, bytesLeft);
                    writer.Write(bytes, bytesWritten, chunk);
                    bytesWritten += chunk;
                    bytesLeft -= chunk;
                    double percent = ((double) bytesWritten / bytes.Length) * 100;
                    Console.Write($"\rProgress: {(int) percent}% | Downloading   ");
                }
                Console.WriteLine();
            }
            await ConvertToMp3(UNCONVERTEDFILEPATH);
            File.Delete(UNCONVERTEDFILEPATH);
            Console.WriteLine("Merging Metadata.");
            await WriteMetaData(CONVERTEDFILEPATH);
            Console.WriteLine("Done.");
        }

        private static async Task ConvertToMp3(string path)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(UNCONVERTEDFILEPATH);
            var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            var conversion = FFmpeg.Conversions.New();
            conversion.AddStream(audioStream);
            conversion.SetOutput(CONVERTEDFILEPATH);
            conversion.SetOverwriteOutput(true);
            conversion.OnProgress += (sender, args) => {
                Console.Write($"\rProgress: {args.Percent}% | Converting   ");
            };
            await conversion.Start();
            Console.WriteLine();
        }

        private static async Task WriteMetaData(string path)
        {
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            TagLib.File file = TagLib.File.Create(CONVERTEDFILEPATH);
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
        }
    }
}
