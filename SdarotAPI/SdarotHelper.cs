using SdarotAPI.Extensions;
using SdarotAPI.Model;
using SdarotAPI.Resources;

namespace SdarotAPI;

public static class SdarotHelper
{
    public static async Task<string> RetrieveSdarotDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.SdarotUrls.SdarotUrlSource)).Trim();
    }

    public static async Task DownloadEpisode(EpisodeMediaDetails episode, string downloadLocation, IProgress<long>? progress = null, CancellationToken ct = default)
    {
        using var handler = new HttpClientHandler() { CookieContainer = episode.Cookies };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36");
        using var file = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write, FileShare.None);
        await client.DownloadAsync(episode.MediaUrl, file, progress, ct);
    }

    public static async Task<long?> GetEpisodeSize(EpisodeMediaDetails episode)
    {
        using var handler = new HttpClientHandler() { CookieContainer = episode.Cookies };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36");
        // Get the http headers first to examine the content length
        using var response = await client.GetAsync(episode.MediaUrl, HttpCompletionOption.ResponseHeadersRead);
        return response.Content.Headers.ContentLength;
    }
}
