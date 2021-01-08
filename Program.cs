using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using VideoLibrary;

namespace SpotiSharp
{
    public static class Program
    {

        public static void Main(string[] args)
        {

            if (Utilities.IsRoot)
            {
                Console.WriteLine("SpotiSharp won't run with root permissions, exiting...");
                Environment.Exit(1);
            }

            Config.Initialize(); // Initialize configuration file

            Console.WriteLine("  ██████  ██▓███   ▒█████  ▄▄▄█████▓ ██▓  ██████  ██░ ██  ▄▄▄       ██▀███   ██▓███  \n" +
                "▒██    ▒ ▓██░  ██▒▒██▒  ██▒▓  ██▒ ▓▒▓██▒▒██    ▒ ▓██░ ██▒▒████▄    ▓██ ▒ ██▒▓██░  ██▒\n" +
                "░ ▓██▄   ▓██░ ██▓▒▒██░  ██▒▒ ▓██░ ▒░▒██▒░ ▓██▄   ▒██▀▀██░▒██  ▀█▄  ▓██ ░▄█ ▒▓██░ ██▓▒\n" +
                "  ▒   ██▒▒██▄█▓▒ ▒▒██   ██░░ ▓██▓ ░ ░██░  ▒   ██▒░▓█ ░██ ░██▄▄▄▄██ ▒██▀▀█▄  ▒██▄█▓▒ ▒\n" +
                "▒██████▒▒▒██▒ ░  ░░ ████▓▒░  ▒██▒ ░ ░██░▒██████▒▒░▓█▒░██▓ ▓█   ▓██▒░██▓ ▒██▒▒██▒ ░  ░\n" +
                "▒ ▒▓▒ ▒ ░▒▓▒░ ░  ░░ ▒░▒░▒░   ▒ ░░   ░▓  ▒ ▒▓▒ ▒ ░ ▒ ░░▒░▒ ▒▒   ▓▒█░░ ▒▓ ░▒▓░▒▓▒░ ░  ░\n" +
                "░ ░▒  ░ ░░▒ ░       ░ ▒ ▒░     ░     ▒ ░░ ░▒  ░ ░ ▒ ░▒░ ░  ▒   ▒▒ ░  ░▒ ░ ▒░░▒ ░     \n" +
                "░  ░  ░  ░░       ░ ░ ░ ▒    ░       ▒ ░░  ░  ░   ░  ░░ ░  ░   ▒     ░░   ░ ░░       \n" +
                "      ░               ░ ░            ░        ░   ░  ░  ░      ░  ░   ░              \n");

            if (args.Length == 0)
            {
                //TODO: Show a Help Page
                Console.WriteLine("SpotiSharp is a Open-Source CLI application made in .NET Core\n" +
                    "Usage: .\\SpotiSharp.exe \"Text | PlaylistUrl | AlbumUrl\"\n" +
                    "No arguments passed...");
                Environment.Exit(1);
            }

            string input = args[0];

            var client = SpotifyHelpers.ConnectToSpotify();
            var trackQueue = new ConcurrentQueue<TrackInfo>();
            var youTube = YouTube.Default;
            if (input.IsSpotifyUrl())
            {
                var (type, url) = input.GetSpotifyUrlId();
                switch (type)
                {
                    case UrlType.Playlist:
                        var taskPlaylist = SpotifyHelpers.QueueSpotifyTracksFromPlaylist(client, url, trackQueue);
                        while (!taskPlaylist.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                DownloadHelpers.DownloadAndConvertTrack(youTube, info);
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            DownloadHelpers.DownloadAndConvertTrack(youTube, info);
                        }
                        break;
                    case UrlType.Album:
                        var taskAlbum = SpotifyHelpers.QueueSpotifyTracksFromAlbum(client, url, trackQueue);
                        while (!taskAlbum.IsCompleted)
                        {
                            while (trackQueue.TryDequeue(out var info))
                            {
                                DownloadHelpers.DownloadAndConvertTrack(youTube, info);
                            }
                            Thread.Sleep(200);
                        }
                        while (trackQueue.TryDequeue(out var info))
                        {
                            DownloadHelpers.DownloadAndConvertTrack(youTube, info);
                        }
                        break;
                    case UrlType.Track:
                        //GetSpotifyTrack();
                        break;
                }
            }
            else
            {
                // SpotifyHelpers.GetSpotifyTrackFromName(client, input);
                var track = SpotifyHelpers.GetSpotifyTrackFromName(client, input).GetAwaiter().GetResult();
                DownloadHelpers.DownloadAndConvertTrack(youTube, track);
            }
        }    
    }
}
