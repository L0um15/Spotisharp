using System.Text.RegularExpressions;

namespace Spotisharp.Client.Resolvers;

public static class YoutubeUriResolver
{
    private static Regex _youtubeUrlRegex = new Regex(@"(http(s)?\:\/\/)?(www\.)?youtube\.com\/watch\?v=[A-Za-z0-9-_]{11}");

    public static bool IsUrlValid(string url)
    {
        return _youtubeUrlRegex.IsMatch(url);
    }

    public static string GetYoutubeVideoID(string url)
    {
        if (_youtubeUrlRegex.IsMatch(url))
        {
            int startIndex = url.LastIndexOf('/') + 9;
            return url.Substring(startIndex, 11);
        }
        return string.Empty;
    }
}

