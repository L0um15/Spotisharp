using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SpotiSharp
{
    public static class Utilities
    {
        public static bool IsSpotifyUrl(this string input)
            => Regex.IsMatch(input, @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)") && input.Contains("open.spotify.com");
        

        public static (UrlType Type, string Url) GetSpotifyUrlId(this string input)
        {
            if (input.Contains("playlist"))
                return (UrlType.Playlist, Regex.Match(input, @"(?<=playlist\/)\w+").Value);
            else if (input.Contains("track"))
                return (UrlType.Track, Regex.Match(input, @"(?<=track\/)\w+").Value);
            else if (input.Contains("album"))
                return (UrlType.Album, Regex.Match(input, @"(?<=album\/)\w+").Value);
            else
                throw new ArgumentException($"The input does not contain a valid Spotify URL");
        }

        public static readonly bool IsRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.UserName == "root" : false;

        public static readonly string ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public enum UrlType
    { 
        Track,
        Playlist,
        Album
    }
}
