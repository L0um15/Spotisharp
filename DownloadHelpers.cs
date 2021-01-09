using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net;
using VideoLibrary;

namespace SpotiSharp
{
    public static class DownloadHelpers
    {
        public static void DownloadAndConvertTrack(this YouTube youtube, TrackInfo trackInfo) {
            var trackPath = Path.Combine(Config.Properties.DownloadPath, $"{trackInfo.Artist} - {trackInfo.Title}.mp3");
            var video = youtube.GetVideo(trackInfo.YoutubeUrl);
            byte[] bytes = video.GetBytes();
            var ffmpeg = new Process()
            { 
                StartInfo = {
                    FileName = Path.Combine(Config.Properties.FFmpegPath, "ffmpeg"),
                    Arguments = $"-i - \"{trackPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }

            };
            ffmpeg.Start();
            var ffmpegInput = ffmpeg.StandardInput.BaseStream;
            ffmpegInput.Write(bytes, 0, bytes.Length);
            ffmpegInput.Close();
            ffmpeg.WaitForExit();
            WriteMetadata(trackInfo, trackPath);
            Console.WriteLine($"Done ::::: {trackInfo.Artist} - {trackInfo.Title}");
        }

        private static void WriteMetadata(TrackInfo trackInfo, string filepath)
        {
            var artist = new string[] { trackInfo.Artist };
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;
            TagLib.File file = TagLib.File.Create(filepath);
            file.Tag.AlbumArtists = artist;
            file.Tag.Performers = artist;
            file.Tag.Composers = artist;
            file.Tag.Copyright = trackInfo.Copyright;
            file.Tag.Lyrics = trackInfo.Lyrics;
            file.Tag.Title = trackInfo.Title;
            file.Tag.Album = trackInfo.Album;
            file.Tag.Track = Convert.ToUInt32(trackInfo.TrackNumber);
            file.Tag.Disc = Convert.ToUInt32(trackInfo.DiscNumber);
            file.Tag.Year = Convert.ToUInt32(trackInfo.Year);
            file.Tag.Comment = trackInfo.SpotifyUrl;
            file.Tag.Genres = new string[] { trackInfo.Genres };
            file.Tag.Pictures = new TagLib.IPicture[] { new TagLib.Picture(trackInfo.AlbumArt) };
            file.Save();
        }
    }
}
