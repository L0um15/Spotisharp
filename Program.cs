using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            if (args.Length == 0)
            {
                //TODO: Show a Help Page
                Console.WriteLine("No arguments passed, exiting...");
                Environment.Exit(1);
            }

            string input = args[0];

            var youtube = YouTube.Default;
            var video = youtube.GetVideo("https://www.youtube.com/watch?v=rwnQs0wks4U"); //THE STORM THAT IS APPOACHING!!!!!!
            var bytes = video.GetBytes();

            var ffmpeg = new Process()
            {
                StartInfo =
                {
                    FileName = "ffmpeg.exe",
                    Arguments = "-i -BuryTheLightDeepWithin.mp3",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            ffmpeg.Start();

            var ffmpegInput = ffmpeg.StandardInput.BaseStream;

            ffmpegInput.Write(bytes, 0, bytes.Length);
            ffmpegInput.Flush();

        }    
    }
}
