using System.Net;
using System.Text.RegularExpressions;

namespace Spotisharp.Client.Services;

public static class YoutubeService
{
    public static async Task<string[]> SearchByText(string query, int limit)
    {
        string[] ids = new string[limit];
        string searchQuery = "https://www.youtube.com/results?search_query=" + query;
        using(HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(searchQuery);
            Match match = Regex.Match(response, "v=([A-Za-z0-9-_]{11})");
            int i = 0;
            while (match.Success)
            {
                if(i >= limit)
                {
                    break;
                }
                ids[i] = match.Groups[1].Value;
                i++;
                match = match.NextMatch();
            }
        }
        return ids;
    }
}