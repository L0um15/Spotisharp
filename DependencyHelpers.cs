using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace SpotiSharp
{
    public static class DependencyHelpers
    {

        public static bool IsFFmpegPresent()
        {
            var files = Directory.GetFiles(Config.Properties.FFmpegPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (Path.GetFileNameWithoutExtension(file.ToLower()) == "ffmpeg")
                    return true;
            }
            return false;
        }

        public static string getFFmpegDownloadUrl(string input = "https://ffbinaries.com/api/v1/version/latest")
        {
            var result = new WebClient().DownloadString(input);
            var parsedDocument = JsonDocument.Parse(result);
            string architecture;
            if (!Utilities.IsLinux)
                architecture = "windows-64";
            else
                architecture = "linux-64";
            var str = parsedDocument.RootElement.GetProperty("bin")
                .GetProperty(architecture)
                .GetProperty("ffmpeg")
                .GetString();
            parsedDocument.Dispose();
            return str;
        }

    }
}
