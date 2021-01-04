using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SpotiSharp
{
    //str clId
    //str clSecret
    //str ffmpeg
    //str download

    public static class Config
    {
        public static string ClientID;
        public static string ClientSecret;
        public static string FFmpegPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string DownloadPath = "";

        private static readonly string configFile = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "config.json");
        
        public static void Initialize()
        {
            if (File.Exists(configFile))
                loadConfig();
        }

        private static void loadConfig()
        {
            var deserialized = JsonSerializer.Deserialize<jsonProxy>(File.ReadAllText(configFile));
            Console.WriteLine(deserialized);
        }

        private class jsonProxy
        { 
            public string ClientID;
            public string ClientSecret;
            public string FFmpegPath;
            public string DownloadPath;

            public override string ToString()
            {
                return $"({ClientID}, {ClientSecret}, {FFmpegPath}, {DownloadPath})";
            }
        }
    }
}
