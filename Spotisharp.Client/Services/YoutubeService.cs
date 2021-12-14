using SpotifyAPI.Web.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Spotisharp.Client.Services;

public static class YoutubeService
{
    public static async Task<string[]> SearchByText(string input, int limit)
    {
        string[] urls = new string[limit];
        string searchQuery = "https://www.youtube.com/results?search_query=" + input;
        using(HttpClient client = new HttpClient())
        {
            string response = await client.GetStringAsync(searchQuery);

            Match match = Regex.Match(response, @"(http(s)?\:\/\/)?(www\.)?youtube\.com\/watch\?v=[A-Za-z0-9-_]{11}");

            int i = 0;
            while (match.Success)
            {
                if (i >= limit)
                {
                    break;
                }
                urls[i] = match.Value;
                i++;
                match = match.NextMatch();
            }
        }
        return urls;
    }

    public static async Task DownloadAsync(string url, string filePath, IProgress<Tuple<long,long>>? progress = null)
    {
        long totalBytesCopied = 0;
        int bytesCopied = 0;
        long fileSize = await GetSize(url);
        long chunkSize = 65536;
        HttpClient httpClient = new HttpClient();
        
        if(fileSize > 0)
        {
            using (FileStream outFile = File.OpenWrite(filePath))
            {
                int segmentCount = (int)Math.Ceiling((decimal)fileSize / chunkSize);

                for(int i = 0; i < segmentCount; i++)
                {
                    long chunkStart = i * chunkSize;
                    long chunkEnd = (i + 1) * chunkSize - 1;
                    
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Range = new RangeHeaderValue(chunkStart, chunkEnd);

                    HttpResponseMessage response =
                        await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode) return;

                    Stream stream = await response.Content.ReadAsStreamAsync();

                    byte[] buffer = new byte[1024];

                    do
                    {
                        bytesCopied = await stream.ReadAsync(buffer, 0, buffer.Length); 
                        outFile.Write(buffer, 0, bytesCopied);
                        totalBytesCopied += bytesCopied;
                        if(progress != null)
                        {
                            progress.Report(new Tuple<long, long>(totalBytesCopied, fileSize));
                        }
                    } while (bytesCopied > 0);

                }
            }
        }
    }

    public static async Task<long> GetSize(string url)
    {
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response
                    = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response.Content.Headers.ContentLength ?? 0;
            }
        }
    }
}