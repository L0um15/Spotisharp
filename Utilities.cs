using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SpotiSharp
{
    public static class Utilities
    {
        public static bool IsSpotifyUrl(this string input)
            => Regex.IsMatch(input, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)") && input.Contains("open.spotify.com");
        

        public static string ExtractSpotifyUrlId(this string input)
        {
            Match match = null;
            if(input.Contains("playlist"))
                match = Regex.Match(input, @"(?<=playlist\/)\w+");
            if(input.Contains("track"))
                match = Regex.Match(input, @"(?<=track\/)\w+");
            return match.Value;
        } 


        public static readonly bool IsRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.UserName == "root" : false;
    }
}
