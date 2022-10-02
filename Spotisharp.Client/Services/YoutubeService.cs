using Spotisharp.Client.Enums;
using Spotisharp.Client.Resolvers;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using VideoLibrary;
using VideoLibrary.Exceptions;

namespace Spotisharp.Client.Services;

public static class YoutubeService
{
    private static HttpClient _httpClient = new HttpClient();

    public static async Task<string[]> SearchByText(string input, int limit)
    {
        string[] urls = new string[limit];

        string safeSearchQuery = "https://www.youtube.com/results?search_query=" + 
            FilenameResolver.RemoveForbiddenChars(input, StringType.Url);

        string response = await _httpClient.GetStringAsync(safeSearchQuery);

        Match match = Regex.Match(response, @"\/watch\?v=[A-Za-z0-9-_]{11}");

        int i = 0;
        while (match.Success)
        {
            if (i >= limit)
            {
                break;
            }
            urls[i] = "https://www.youtube.com" + match.Value;
            i++;
            match = match.NextMatch();
        }
        return urls;
    }

    public static async Task<YouTubeVideo?> GetAudioOnly(string url)
    {
        try
        {
            YouTube youtubeEngine = YouTube.Default;
            IEnumerable<YouTubeVideo> videos = await youtubeEngine.GetAllVideosAsync(url);
            YouTubeVideo audioTrack = videos.First(v =>
                v.AdaptiveKind == AdaptiveKind.Audio
            );

            return audioTrack;
        }
        catch (UnavailableStreamException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public static async Task<Stream> GetStreamAsync(string url, IProgress<Tuple<long, long>>? progress = null)
    {
        long fileSize = await GetSize(url) ?? 0;
        long totalBytesCopied = 0;
        long chunkSize = 65535; // 64KB
        Stream output = new MemoryStream();

        if (fileSize > 0)
        {
            int segmentCount = (int)Math.Ceiling(1.0 * fileSize / chunkSize);
            for (int i = 0; i < segmentCount; i++)
            {
                long from = i * chunkSize;
                long to = (i + 1) * chunkSize - 1;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(from, to);
                using (request)
                {
                    HttpResponseMessage response =
                        await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    using (stream)
                    {
                        byte[] buffer = new byte[80 * 1024];
                        int bytesCopied = 0;
                        do
                        {

                            bytesCopied = await stream.ReadAsync(buffer, 0, buffer.Length);
                            output.Write(buffer, 0, bytesCopied);
                            if (progress != null)
                            {
                                progress.Report(Tuple.Create(totalBytesCopied, fileSize));
                            }
                            totalBytesCopied += bytesCopied;

                        } while (bytesCopied > 0);
                    }
                }
            }
        }
        return output;
    }

    public static async Task<long?> GetSize(string url)
    {
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
        {
            HttpResponseMessage response
                = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return response.Content.Headers.ContentLength;
        }
    }
}