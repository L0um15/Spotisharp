using SpotifyAPI.Web;
using Spotisharp.Client;
using Spotisharp.Client.Resolvers;

SpotifyClient? client = await SpotifyAuthentication.CreateSpotifyClient();

if (client == null)
{
    Console.WriteLine("Client timed out");
    return;
}

var searchResponse = await client.Search.Item(new SearchRequest(SearchRequest.Types.Track, "Enemy Imagine dragons"));

if (searchResponse == null)
{
    Console.WriteLine("Unable to search for track");
    return;
}

Console.WriteLine("\n\n"+searchResponse.Tracks.Items[0].ExternalUrls["spotify"]);

Console.WriteLine(SpotifyUriResolver.GetID("https://open.spotify.com/track/5nyef8bHyXaglyArVUNlre?si=906a2ae6cd2842c7"));