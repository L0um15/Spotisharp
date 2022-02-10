using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Spotisharp.Client.Services;

public static class UpdateService
{
    private static readonly ProductInfoHeaderValue _userAgent = new ProductInfoHeaderValue("Spotisharp", "v3");
    private static readonly string _currentVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    private static JsonDocument? _jDoc = null;

    private static async Task<string> GetJsonData()
    {
        string url = "https://api.github.com/repos/L0um15/Spotisharp/releases/latest";
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
            return await response.Content.ReadAsStringAsync();
        }
    }

    public static async Task<bool> CheckForUpdates()
    {
        string jsonString = await GetJsonData();
        _jDoc = JsonDocument.Parse(jsonString);
        if (_jDoc != null)
        {
            if (_jDoc.RootElement.TryGetProperty("tag_name", out JsonElement latestVersion))
            {
                string shortVersion = _currentVersion.Substring(0, _currentVersion.LastIndexOf('.'));
                if (latestVersion.ToString() != shortVersion)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static string GetChangelog()
    {
        if(_jDoc != null)
        {
            if(_jDoc.RootElement.TryGetProperty("body", out JsonElement body))
            {
                return body.ToString();
            }
        }
        return "";
    }
}

