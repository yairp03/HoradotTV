namespace SdarotAPI;

public static class SdarotHelper
{
    public static async Task<string> RetrieveSdarotDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.SdarotUrls.SdarotUrlSource)).Trim();
    }

    public static async Task<string> GetSdarotTestUrl()
    {
        return $"https://{await RetrieveSdarotDomain()}/watch/1";
    }

    public static async Task DownloadEpisode(EpisodeMediaDetails episode, string downloadLocation, IProgress<long>? progress = null, CancellationToken ct = default)
    {
        using var handler = new HttpClientHandler() { CookieContainer = episode.Cookies };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);
        using var file = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write, FileShare.None);
        await client.DownloadAsync(episode.MediaUrl, file, progress, ct);
    }

    public static async Task<long?> GetEpisodeSize(EpisodeMediaDetails episode)
    {
        using var handler = new HttpClientHandler() { CookieContainer = episode.Cookies };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);
        // Get the http headers first to examine the content length
        using var response = await client.GetAsync(episode.MediaUrl, HttpCompletionOption.ResponseHeadersRead);
        return response.Content.Headers.ContentLength;
    }
}
