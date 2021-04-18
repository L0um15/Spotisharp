using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using YoutubeExplode;

namespace SpotiSharp
{
    public static class Program
    {

        public static void Main(string[] args)
        {

            Console.WriteLine("  ██████  ██▓███   ▒█████  ▄▄▄█████▓ ██▓  ██████  ██░ ██  ▄▄▄       ██▀███   ██▓███  \n" +
                "▒██    ▒ ▓██░  ██▒▒██▒  ██▒▓  ██▒ ▓▒▓██▒▒██    ▒ ▓██░ ██▒▒████▄    ▓██ ▒ ██▒▓██░  ██▒\n" +
                "░ ▓██▄   ▓██░ ██▓▒▒██░  ██▒▒ ▓██░ ▒░▒██▒░ ▓██▄   ▒██▀▀██░▒██  ▀█▄  ▓██ ░▄█ ▒▓██░ ██▓▒\n" +
                "  ▒   ██▒▒██▄█▓▒ ▒▒██   ██░░ ▓██▓ ░ ░██░  ▒   ██▒░▓█ ░██ ░██▄▄▄▄██ ▒██▀▀█▄  ▒██▄█▓▒ ▒\n" +
                "▒██████▒▒▒██▒ ░  ░░ ████▓▒░  ▒██▒ ░ ░██░▒██████▒▒░▓█▒░██▓ ▓█   ▓██▒░██▓ ▒██▒▒██▒ ░  ░\n" +
                "▒ ▒▓▒ ▒ ░▒▓▒░ ░  ░░ ▒░▒░▒░   ▒ ░░   ░▓  ▒ ▒▓▒ ▒ ░ ▒ ░░▒░▒ ▒▒   ▓▒█░░ ▒▓ ░▒▓░▒▓▒░ ░  ░\n" +
                "░ ░▒  ░ ░░▒ ░       ░ ▒ ▒░     ░     ▒ ░░ ░▒  ░ ░ ▒ ░▒░ ░  ▒   ▒▒ ░  ░▒ ░ ▒░░▒ ░     \n" +
                "░  ░  ░  ░░       ░ ░ ░ ▒    ░       ▒ ░░  ░  ░   ░  ░░ ░  ░   ▒     ░░   ░ ░░       \n" +
                $"      ░               ░ ░            ░        ░   ░  ░  ░      ░  ░   ░       {Utilities.ApplicationVersion}\n");

            if (Utilities.IsRoot)
            {
                Console.WriteLine("SpotiSharp won't run with root permissions, exiting...");
                Environment.Exit(1);
            }

            Config.Initialize(); // Initialize configuration file

            if (!DependencyHelpers.IsFFmpegPresent())
            {
                Console.WriteLine("FFmpeg not found. Downloading...");
                string ffmpegZipPath = Path.Combine(Config.Properties.FFmpegPath, "ffmpeg.zip");
                new WebClient().DownloadFile(
                    DependencyHelpers.getFFmpegDownloadUrl(),
                    ffmpegZipPath
                );
                Utilities.UnZip(ffmpegZipPath, Config.Properties.FFmpegPath, true);
                File.Delete(ffmpegZipPath);
                if (Utilities.IsLinux) {
                    Console.WriteLine("This message is visible for linux users after downloading FFmpeg.\n" +
                        "Make sure you granted execution permissions for ffmpeg before running SpotiSharp again.\n" +
                        "SpotiSharp will now close.");
                    Environment.Exit(1);
                }
            }

            string version = Utilities.CheckForLatestApplicationVersion();

            if (version != null)
                Console.WriteLine($"Out of date!: {version}\n");

            if (args.Length == 0)
            {
                Console.WriteLine("SpotiSharp is a Open-Source CLI application made in .NET Core\n" +
                    "Usage: .\\SpotiSharp.exe \"Text | PlaylistUrl | AlbumUrl\"\n" +
                    "No arguments passed...");
                Environment.Exit(1);
            }

            string input = args[0];

            Console.WriteLine("Connecting To Spotify...");
            var client = SpotifyHelpers.ConnectToSpotify();
            var trackQueue = new ConcurrentQueue<TrackInfo>();

            var youTube = new YoutubeClient();

            Console.WriteLine("Making requests to Spotify...");
            if (input.IsSpotifyUrl())
            {
                var (type, spotifyId) = input.GetSpotifyId();
                switch (type)
                {
                    case UrlType.Playlist:
                        var taskPlaylist = client.QueueSpotifyTracksFromPlaylist(spotifyId, trackQueue);
                        while (!taskPlaylist.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                Console.Write($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
                                Console.Write($"[Queue: {trackQueue.Count}]".MoveToRight());
                                youTube.DownloadAndConvertTrack(info);
                                Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            Console.Write($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
                            Console.Write($"[Queue: {trackQueue.Count}]".MoveToRight());
                            youTube.DownloadAndConvertTrack(info);
                            Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
                        }
                        break;
                    case UrlType.Album:
                        var taskAlbum = client.QueueSpotifyTracksFromAlbum(spotifyId, trackQueue);
                        while (!taskAlbum.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                Console.Write($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
                                Console.Write($"[Queue: {trackQueue.Count}]".MoveToRight());
                                youTube.DownloadAndConvertTrack(info);
                                Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            Console.Write($"Downloading ::::: {info.Artist} - {info.Title}".Truncate());
                            Console.Write($"[Queue: {trackQueue.Count}]".MoveToRight());
                            youTube.DownloadAndConvertTrack(info);
                            Console.WriteLine($"Done        ::::: {info.Artist} - {info.Title}".Truncate());
                        }
                        break;
                    case UrlType.Track:
                        var track = client.GetSpotifyTrack(input).GetAwaiter().GetResult();
                        if (track == null)
                            Environment.Exit(1);
                        Console.WriteLine($"Downloading ::::: {track.Artist} - {track.Title}".Truncate());
                        youTube.DownloadAndConvertTrack(track);
                        Console.WriteLine($"Done        ::::: {track.Artist} - {track.Title}".Truncate());
                        break;
                }
            }
            else
            {
                var track = client.GetSpotifyTrack(input).GetAwaiter().GetResult();
                if (track == null)
                    Environment.Exit(1);
                Console.WriteLine($"Downloading ::::: {track.Artist} - {track.Title}".Truncate());
                youTube.DownloadAndConvertTrack(track);
                Console.WriteLine($"Done        ::::: {track.Artist} - {track.Title}".Truncate());
            }
        }    
    }
}
