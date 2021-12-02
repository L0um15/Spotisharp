using Spotisharp.Client.Enums;
using System.Text.RegularExpressions;

namespace Spotisharp.Client.Resolvers;

public static class SpotifyUriResolver
{
    private static readonly Regex _spotifyUriRegex = new Regex(@"http(s)?\:\/\/open\.spotify\.com\/(track|playlist|album)\/.{22}");
    public static bool IsUriValid(string uri)
    {
        return _spotifyUriRegex.IsMatch(uri);
    }
    public static SpotifyUriType GetUriType(string uri)
    {
        if (_spotifyUriRegex.IsMatch(uri))
        {
            if (uri.Contains("track"))
            {
                return SpotifyUriType.Track;
            }
            else if (uri.Contains("playlist"))
            {
                return SpotifyUriType.Playlist;
            }
            else if (uri.Contains("album"))
            {
                return SpotifyUriType.Album;
            }
        }
        throw new ArgumentException("SpotifyUri is invalid, is it missing something?");
    }
    public static string GetID(string uri)
    {
        return uri.Substring(uri.LastIndexOf("/") + 1, 22);
    }
}
