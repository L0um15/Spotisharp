using SpotifyAPI.Web;
using Spotisharp.Client.Resolvers;

if (ConfigManager.Init())
{
    Console.WriteLine("Loaded config file!");
}

var client = await SpotifyAuthentication.CreateSpotifyClient();

if(client == null)
{
    return;
}

string url = "https://www.youtube.com/watch?v=FYR2PjVJXUM";