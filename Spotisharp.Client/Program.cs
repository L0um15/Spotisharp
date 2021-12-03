using SpotifyAPI.Web;
using Spotisharp.Client;
using Spotisharp.Client.Enums;
using Spotisharp.Client.Resolvers;

string uri = "https://open.spotify.com/track/5nyef8bHyXaglyArVUNlre?si=906a2ae6cd2842c70";

var uriType = SpotifyUriResolver.GetUriType(uri);

if (uriType == SpotifyUriType.None)
{
    return;
}

var id = SpotifyUriResolver.GetID(uri, uriType);

Console.WriteLine(id);