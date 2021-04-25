using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;

namespace SpotiSharp
{
    public static class DownloadHelpers
    {
        public static void DownloadAndConvertTrack(this YoutubeClient youtube, TrackInfo trackInfo) {
            var trackDirectory = Path.Combine(Config.Properties.DownloadPath, trackInfo.Playlist);
            Directory.CreateDirectory(trackDirectory);
            var trackPath = Path.Combine(trackDirectory,$"{trackInfo.Artist} - {trackInfo.Title}.mp3");
            StreamManifest manifest = null;
            for(int i = 0; i < trackInfo.YoutubeIds.Length; i++)
            {
                try
                {
                    manifest = youtube.Videos.Streams.GetManifestAsync(trackInfo.YoutubeIds[i]).GetAwaiter().GetResult();
                    break;
                }
                catch (VideoUnplayableException)
                {
                    // Oops... I hope next url will be not restricted.
                }
            }

            if(manifest == null)
            {
                // This should be never displayed. Otherwise you are unlucky.
                Console.WriteLine($"Failed      ::::: {trackInfo.Artist} - {trackInfo.Title}: source is restricted".Truncate());
                return;
            }

            IStreamInfo streamInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var ffmpeg = new Process()
            {
                StartInfo = {
                    FileName = Path.Combine(Config.Properties.FFmpegPath, "ffmpeg"),
                    Arguments = $"-hide_banner -loglevel quiet -i - -q:a 0 \"{trackPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            ffmpeg.Start();
            var ffmpegInput = ffmpeg.StandardInput.BaseStream;
            youtube.Videos.Streams.CopyToAsync(streamInfo, ffmpegInput).GetAwaiter().GetResult();
            ffmpegInput.Close();
            ffmpeg.WaitForExit();
            WriteMetadata(trackInfo, trackPath);
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
