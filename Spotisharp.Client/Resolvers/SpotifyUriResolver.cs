using Spotisharp.Client.Enums;
using System.Text.RegularExpressions;

namespace Spotisharp.Client.Resolvers;

public static class SpotifyUriResolver
{
    private static readonly Regex _spotifyUrlRegex = new Regex(@"http(s)?\:\/\/open\.spotify\.com\/(track|playlist|album)\/.{22}");
    private static readonly Regex _spotifyUrnRegex = new Regex(@"spotify\:(track|playlist|album)\:[0-9A-Za-z]{22}");
    
    public static bool IsUriValid(string uri)
    {
        return _spotifyUrlRegex.IsMatch(uri) || _spotifyUrnRegex.IsMatch(uri);
    }

    public static SpotifyUriType GetUriType(string uri)
    {
        if (_spotifyUrlRegex.IsMatch(uri))
        {
            return SpotifyUriType.Url;
        }
        else if(_spotifyUrnRegex.IsMatch(uri))
        {
            return SpotifyUriType.Urn;
        }
        else
        {
            return SpotifyUriType.None;
        }
    }

    public static SpotifyBrowseCategory GetBrowseCategory(string uri)
    {
        if (uri.Contains("track"))
        {
            return SpotifyBrowseCategory.Track;
        }
        else if (uri.Contains("playlist"))
        {
            return SpotifyBrowseCategory.Playlist;
        }
        else if (uri.Contains("album"))
        {
            return SpotifyBrowseCategory.Album;
        }
        else
        {
            return SpotifyBrowseCategory.None;
        }
    }

    public static string GetID(string uri, SpotifyUriType spotifyUriType)
    {
        if (spotifyUriType == SpotifyUriType.Url)
        {
            return uri.Substring(uri.LastIndexOf("/") + 1, 22);
        }
        else if (spotifyUriType == SpotifyUriType.Urn)
        {
            return uri.Substring(uri.LastIndexOf(':') + 1, 22);
        }
        else
        {
            return string.Empty;
        }
    }
}
