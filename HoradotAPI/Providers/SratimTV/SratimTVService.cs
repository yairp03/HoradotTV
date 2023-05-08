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

        var searchResult = await JsonSerializer.DeserializeAsync<SratimTVSearchResult>(
            await (await httpClient.PostAsync(Constants.Urls.MovieSearchUrl, new FormUrlEncodedContent(data))).Content
                .ReadAsStreamAsync());

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
        throw new NotImplementedException();

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress) =>
        throw new NotImplementedException();

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress,
        CancellationToken ct) => throw new NotImplementedException();

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream) =>
        throw new NotImplementedException();

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress) => throw new NotImplementedException();

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream, IProgress<long>? progress,
        CancellationToken ct) =>
        throw new NotImplementedException();

    private static class Constants
    {
        internal const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        internal const string LoginMessage = "התחברות לאתר";

        internal const string ImagesFormat = "jpg";

        internal const int QueryMinLength = 3;

        internal static class Urls
        {
            internal const string DomainSource =
                "https://raw.githubusercontent.com/yairp03/HoradotTV/master/Resources/SratimTV-domain.txt";

            internal const string ConnectionProblemGuide =
                "https://github.com/yairp03/HoradotTV/wiki/SratimTV-connection-problem";

            internal static string BaseDomain { get; set; } = "";
            internal static string HomeUrl => $"https://{BaseDomain}/";
            internal static string ApiUrl => $"https://api.{BaseDomain}/";

            internal static string LoginUrl => $"{HomeUrl}login";
            internal static string MovieUrl => $"{HomeUrl}movie/";
            internal static string MovieSearchUrl => $"{ApiUrl}movie/search";
            internal static string ImageUrl => $"https://static.{BaseDomain}/movies/";
            internal static string TestUrl => $"{MovieUrl}7334";
            internal static string AjaxWatchUrl => $"{HomeUrl}ajax/watch";
            internal static string AjaxAllShowsUrl => $"{HomeUrl}ajax/index?srl=1";
        }

        internal static class XPathSelectors
        {
            internal const string MainPageLoginPanelButton = "//*[@id=\"slideText\"]/p/button";

            internal const string ShowPageSeason = "//*[@id=\"season\"]/li";

            internal const string AjaxEpisode = "/li";
        }
    }
}
