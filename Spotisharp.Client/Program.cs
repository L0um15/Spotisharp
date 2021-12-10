using SpotifyAPI.Web;
using Spotisharp.Client.Resolvers;
using Spotisharp.Client.Services;

if (ConfigManager.Init())
{
    Console.WriteLine("Loaded config file!");
}

var client = await SpotifyAuthentication.CreateSpotifyClient();

if(client == null)
{
    return;
}