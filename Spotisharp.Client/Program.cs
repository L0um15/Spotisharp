if (ConfigManager.Init())
{
    Console.WriteLine("Loaded config file!");
}

var client = await SpotifyAuthentication.CreateSpotifyClient();

if(client != null)
{
    Console.WriteLine("YES!");
}