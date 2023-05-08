namespace HoradotAPI.Providers.SratimTV;

public class SratimTVService : IContentProvider
{
    private readonly HttpClient httpClient;
    private bool isInitialized;

    public SratimTVService()
    {
        httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });
        httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);
    }

    public string Name => "SratimTV";
    public Task<(bool success, string errorMessage)> InitializeAsync() => InitializeAsync(true);

    public async Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks)
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }

        Constants.Urls.BaseDomain = await RetrieveSratimTVDomain();
        httpClient.DefaultRequestHeaders.Referrer = new Uri(Constants.Urls.HomeUrl);

        if (doChecks && !await TestConnection())
        {
            return (false, $"SratimTV is blocked. Please refer to {Constants.Urls.ConnectionProblemGuide}");
        }

        isInitialized = true;

        return (true, string.Empty);
    }

    private static async Task<string> RetrieveSratimTVDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.Urls.DomainSource)).Trim();
    }

    private async Task<bool> TestConnection()
    {
        try
        {
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, Constants.Urls.TestUrl));
        }
        catch (HttpRequestException)
        {
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<MediaInformation>> SearchAsync(string query)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        if (query.Length < Constants.QueryMinLength)
        {
            return Enumerable.Empty<MediaInformation>();
        }

        Dictionary<string, string> data = new()
        {
            ["term"] = query
        };

        SratimTVSearchResult? searchResult = null;
        for (int i = 0; i < Constants.SearchRetries; i++)
        {
            try
            {
                searchResult = await JsonSerializer.DeserializeAsync<SratimTVSearchResult>(
                    await (await httpClient.PostAsync(Constants.Urls.ApiMovieSearchUrl,
                        new FormUrlEncodedContent(data))).Content.ReadAsStreamAsync());
                break;
            }
            catch
            {
                // Try again
            }
        }


        if (searchResult is null)
        {
            return Enumerable.Empty<MediaInformation>();
        }

        foreach (var movie in searchResult.Results)
        {
            movie.ImageUrl = $"{Constants.Urls.ImageUrl}{movie.Id}.{Constants.ImagesFormat}";
        }

        return searchResult.Results;
    }

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media) =>
        PrepareDownloadAsync(media, null);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress) =>
        PrepareDownloadAsync(media, progress, default(CancellationToken));

    public async Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media,
        IProgress<double>? progress, CancellationToken ct)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        if (media is not MovieInformation movie)
        {
            throw new ArgumentException("Media is not movie.", nameof(media));
        }

        // Start 30 seconds loading
        string token = await httpClient.GetStringAsync(Constants.Urls.ApiMoviePreWatchUrl, ct);

        for (double i = 0.0; i < Constants.WaitAmount; i += 1.0 / Constants.WaitUps)
        {
            await Task.Delay(1000 / Constants.WaitUps, ct);
            progress?.Report(i);
        }

        // Fetch episode media details
        var mediaDetails = await httpClient.GetFromJsonAsync<SratimTVMediaDetails>(GetMovieUrl(movie, token), ct);

        if (mediaDetails is null)
        {
            return null;
        }

        return new MediaDownloadInformation
        {
            Information = media,
            Resolutions = mediaDetails.Resolutions.ToImmutableDictionary()
        };
    }

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream) =>
        DownloadAsync(media, resolution, stream, null);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress) => DownloadAsync(media, resolution, stream, progress, default(CancellationToken));

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream, IProgress<long>? progress,
        CancellationToken ct) =>
        httpClient.DownloadAsync($"https:{media.Resolutions[resolution]}", stream, progress, ct);

    private static string GetMovieUrl(MediaInformation movie, string token) =>
        $"{Constants.Urls.ApiMovieWatchUrl}/id/{movie.Id}/token/{token}";

    private static class Constants
    {
        internal const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        internal const string ImagesFormat = "jpg";

        internal const int QueryMinLength = 3;

        internal const double WaitAmount = 30.0;
        internal const int WaitUps = 10;

        internal const int SearchRetries = 3;

        internal static class Urls
        {
            internal const string DomainSource =
                "https://raw.githubusercontent.com/yairp03/HoradotTV/master/Resources/SratimTV-domain.txt";

            internal const string ConnectionProblemGuide =
                "https://github.com/yairp03/HoradotTV/wiki/SratimTV-connection-problem";

            internal static string BaseDomain { get; set; } = "";

            private static string ApiUrl => $"https://api.{BaseDomain}";
            private static string ApiMovieUrl => $"{ApiUrl}/movie";
            internal static string ApiMovieSearchUrl => $"{ApiMovieUrl}/search";
            internal static string ApiMoviePreWatchUrl => $"{ApiMovieUrl}/preWatch";
            internal static string ApiMovieWatchUrl => $"{ApiMovieUrl}/watch";

            internal static string HomeUrl => $"https://{BaseDomain}";
            private static string MovieUrl => $"{HomeUrl}/movie/";
            internal static string TestUrl => $"{MovieUrl}7334";

            internal static string ImageUrl => $"https://static.{BaseDomain}/movies/";
        }
    }
}
