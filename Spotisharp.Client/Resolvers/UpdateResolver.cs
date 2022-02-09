using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Spotisharp.Client.Resolvers;

public static class UpdateResolver
{
    private static ProductInfoHeaderValue _userAgent = new ProductInfoHeaderValue("Spotisharp", "v3");
    private static string _currentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    private static async Task<string> GetJsonData()
    {
        string url = "https://api.github.com/repos/L0um15/Spotisharp/releases/latest";
        using(HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public static async Task<bool> CheckForUpdates()
    {
        string jString = await GetJsonData();
        JsonDocument jDoc = JsonDocument.Parse(jString);
        if(jDoc.RootElement.TryGetProperty("tag_name", out JsonElement latestVersion))
        {
            string shortCurrentVersion = _currentVersion.Substring(0, _currentVersion.LastIndexOf('.'));
            if(latestVersion.ToString() != shortCurrentVersion)
            {
                return true;
            }
        }
        return false;
    }


}
