using HtmlAgilityPack;

namespace Spotisharp.Client.Services;

public static class MusixmatchService
{
    private static readonly string _siteUrl = "https://www.musixmatch.com";

    public static async Task<string> SearchLyricsFromText(string query)
    {
        string searchQuery = _siteUrl + "/search/" + query;
        using (HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(searchQuery);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            
            var foundList = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'tracks')]");
            
            if(foundList == null)
            {
                return string.Empty;
            }

            var foundTrack = foundList.SelectSingleNode("//a[@class='title'][1]");

            if(foundTrack == null)
            {
                return string.Empty;
            }

            string trackUrl = _siteUrl + foundTrack.Attributes["href"].Value;

            response = await client.GetStringAsync(trackUrl);

            htmlDoc.LoadHtml(response);

            var lyrics = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='mxm-lyrics']/span"); // Approved Lyrics
            if (lyrics == null)
                lyrics = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__ok']"); // Correct Lyrics waiting for approval
            if (lyrics == null)
                lyrics = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='lyrics__content__warning']"); // Incorrect Lyrics waiting for review.

            return lyrics?.InnerText ?? string.Empty;
        }
    }
}
