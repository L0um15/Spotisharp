using SpotifyAPI.Web;

if (ConfigManager.Init())
{
    Console.WriteLine("Loaded config file!");
}

var client = await SpotifyAuthentication.CreateSpotifyClient();

if(client == null)
{
    return;
}

var searchResponse = await client.Search.Item(new SearchRequest(SearchRequest.Types.Track, "enemy imagine dragons"));

if(searchResponse != null)
{
    var song = searchResponse.Tracks?.Items?[0].ExternalUrls["spotify"];
    Console.WriteLine(song);
}