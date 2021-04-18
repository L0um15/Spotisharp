using System;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SpotiSharp
{
    public static class Utilities
    {
        public static bool IsSpotifyUrl(this string input)
            => Regex.IsMatch(input, @"http(s)?\:\/\/open\.spotify\.com\/(track|playlist|album)\/.+$");

        public static (UrlType Type, string Id) GetSpotifyId(this string input)
        {
            if (input.Contains("playlist"))
                return (UrlType.Playlist, ExtractSpotifyId(input));
            else if (input.Contains("track"))
                return (UrlType.Track, ExtractSpotifyId(input));
            else if (input.Contains("album"))
                return (UrlType.Album, ExtractSpotifyId(input));
            else
                throw new ArgumentException($"The input does not contain a valid Spotify URL");
        }

        private static string ExtractSpotifyId(string input)
        {
            int startIndex = input.LastIndexOf('/') + 1;
            return input.Substring(startIndex,22);
        }

        public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static readonly bool IsRoot = IsLinux ? Environment.UserName == "root" : false;

        public static readonly string ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static void UnZip(string path, string destination, bool overwrite) 
            => ZipFile.ExtractToDirectory(path, destination, overwrite);
        public static string CheckForLatestApplicationVersion()
        {
            try
            {
                string latestVersion = new WebClient().DownloadString("https://raw.githubusercontent.com/L0um15/SpotiSharp/main/version.txt");
                if (ApplicationVersion != latestVersion)
                    return latestVersion;
            }
            catch (WebException)
            {
                Console.WriteLine("Something went wrong with Github");
            }
            return null;
        }

        public static string MakeSafe(this string input)
        {
            return string.Create(input.Length, input, (chars, buffer)=> 
            {
                for(int i = 0; i < buffer.Length; i++)
                {
                    switch (buffer[i])
                    {
                        // Fall trough
                        case '/':
                        case '\\':
                        case '*':
                        case ':':
                        case '?':
                        case '<':
                        case '>':
                        case '\"':
                        case '|':
                            chars[i] = ' ';
                            break;
                        default:
                            chars[i] = buffer[i];
                            break;
                    }
                }
            });
        }

        /*public static string MakeSafe(this string input) 
            => Regex.Replace(input, @"[\/\\\?\*\<\>\|\:\""]", " ");
        public static string MakeUriSafe(this string input)
            => Regex.Replace(input, @"[\s+\&]", "%20");*/
    }

    public enum UrlType
    { 
        Track,
        Playlist,
        Album
    }
}
