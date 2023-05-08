namespace HoradotAPI.Providers.SdarotTV;

public class SdarotTVService : BaseShowProvider, IShowProvider, IAuthContentProvider
{
    private readonly HttpClient httpClient;
    private bool isInitialized;
    private bool isLoggedIn;

    public SdarotTVService()
    {
        httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });
        httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);
    }

    public override string Name => "SdarotTV";


    public override async Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks)
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }

        Constants.Urls.BaseDomain = await RetrieveSdarotTVDomain();
        httpClient.DefaultRequestHeaders.Referrer = new Uri(Constants.Urls.HomeUrl);

        if (doChecks && !await TestConnection())
        {
            return (false, $"SdarotTV is blocked. Please refer to {Constants.Urls.ConnectionProblemGuide}");
        }

        isInitialized = true;

        return (true, string.Empty);
    }

    public override async Task<IEnumerable<MediaInformation>> SearchAsync(string query)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        List<SdarotTVShowInformation>? result = null;
        for (int i = 0; i < Constants.SearchRetries; i++)
        {
            try
            {
                result = (await JsonSerializer.DeserializeAsync<List<SdarotTVShowInformation>>(
                    await httpClient.GetStreamAsync(Constants.Urls.AjaxAllShowsUrl)))?.Where(x =>
                    x.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                    x.NameHe.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();
                break;
            }
            catch
            {
                // Try again
            }
        }

        if (result is null)
        {
            return Enumerable.Empty<MediaInformation>();
        }

        foreach (var show in result)
        {
            show.ImageUrl = $"{Constants.Urls.ImageUrl}{show.ImageName}";
        }

        return result;
    }

    public override async Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media,
        IProgress<double>? progress, CancellationToken ct)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        if (!await IsLoggedIn())
        {
            throw new NotAuthenticatedException(Name);
        }

        if (media is not EpisodeInformation episode)
        {
            throw new ArgumentException("Media is not episode.", nameof(media));
        }

        // Start 30 seconds loading
        Dictionary<string, string> data = new()
        {
            ["preWatch"] = true.ToString(),
            ["SID"] = episode.Show.Id.ToString(),
            ["season"] = episode.Season.Id.ToString(),
            ["ep"] = episode.Id.ToString()
        };

        var postResult = await httpClient.PostAsync(Constants.Urls.AjaxWatchUrl, new FormUrlEncodedContent(data), ct);
        string token = await postResult.Content.ReadAsStringAsync(ct);

        for (double i = 0.0; i < Constants.WaitAmount; i += 1.0 / Constants.WaitUps)
        {
            await Task.Delay(1000 / Constants.WaitUps, ct);
            progress?.Report(i);
        }

        // Fetch episode media details
        data = new Dictionary<string, string>
        {
            ["watch"] = false.ToString(),
            ["token"] = token,
            ["serie"] = episode.Show.Id.ToString(),
            ["season"] = episode.Season.Id.ToString(),
            ["episode"] = episode.Id.ToString(),
            ["type"] = "episode"
        };

        postResult = await httpClient.PostAsync(Constants.Urls.AjaxWatchUrl, new FormUrlEncodedContent(data), ct);

        var mediaDetails = await postResult.Content.ReadFromJsonAsync<SdarotTVMediaDetails>(cancellationToken: ct);

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

    public override Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress, CancellationToken ct) =>
        httpClient.DownloadAsync($"https:{media.Resolutions[resolution]}", stream, progress, ct);

    public Task<bool> IsLoggedIn() => Task.FromResult(isLoggedIn);

    public async Task<bool> Login(string username, string password)
    {
        Dictionary<string, string> data = new()
        {
            ["username"] = username,
            ["password"] = password,
            ["submit_login"] = ""
        };

        _ = await httpClient.PostAsync(Constants.Urls.LoginUrl, new FormUrlEncodedContent(data));

        isLoggedIn = await CheckLoggedIn();

        return isLoggedIn;
    }

    public override async Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        string showHtml = await httpClient.GetStringAsync($"{Constants.Urls.WatchUrl}{show.Id}");
        var doc = new HtmlDocument();
        doc.LoadHtml(showHtml);

        var seasonElements = doc.DocumentNode.SelectNodes(Constants.XPathSelectors.ShowPageSeason);

        if (seasonElements is null || seasonElements.Count == 0)
        {
            return Enumerable.Empty<SeasonInformation>();
        }

        List<SeasonInformation> seasons = new();
        foreach (var (season, i) in seasonElements.Select((season, i) => (season, i)))
        {
            int seasonId = season.GetAttributeValue("data-season", 0);
            string? seasonName = season.SelectSingleNode("a").InnerText;
            seasons.Add(new SeasonInformation
            {
                Id = seasonId,
                Index = i,
                Name = seasonName,
                ProviderName = Name,
                Show = show
            });
        }

        return seasons;
    }

    public override async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        string episodesHtml =
            await httpClient.GetStringAsync(
                $"{Constants.Urls.AjaxWatchUrl}?episodeList={season.Show.Id}&season={season.Id}");
        var doc = new HtmlDocument();
        doc.LoadHtml(episodesHtml);

        var episodeElements = doc.DocumentNode.SelectNodes(Constants.XPathSelectors.AjaxEpisode);

        if (episodeElements is null || episodeElements.Count == 0)
        {
            return Enumerable.Empty<EpisodeInformation>();
        }

        List<EpisodeInformation> episodes = new();
        foreach (var (episode, i) in episodeElements.Select((episode, i) => (episode, i)))
        {
            int episodeId = episode.GetAttributeValue("data-episode", 0);
            string? episodeName = episode.SelectSingleNode("a").InnerText;
            episodes.Add(new EpisodeInformation
            {
                Id = episodeId,
                Index = i,
                Name = episodeName,
                ProviderName = Name,
                Season = season
            });
        }

        return episodes;
    }

    private static async Task<string> RetrieveSdarotTVDomain()
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

    private async Task<bool> CheckLoggedIn()
    {
        string searchHtml = await httpClient.GetStringAsync(Constants.Urls.HomeUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(searchHtml);

        var loginPanelButton = doc.DocumentNode.SelectSingleNode(Constants.XPathSelectors.MainPageLoginPanelButton);

        return loginPanelButton != null
            ? loginPanelButton.InnerText != Constants.LoginMessage
            : throw new ElementNotFoundException(nameof(loginPanelButton));
    }

    private static class Constants
    {
        internal const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        internal const string LoginMessage = "התחברות לאתר";

        internal const double WaitAmount = 30.0;
        internal const int WaitUps = 10;

        internal const int SearchRetries = 3;

        internal static class Urls
        {
            internal const string DomainSource =
                "https://raw.githubusercontent.com/yairp03/HoradotTV/master/Resources/SdarotTV-domain.txt";

            internal const string ConnectionProblemGuide =
                "https://github.com/yairp03/HoradotTV/wiki/SdarotTV-connection-problem";

            internal static string BaseDomain { get; set; } = "";
            internal static string HomeUrl => $"https://{BaseDomain}/";
            internal static string LoginUrl => $"{HomeUrl}login";
            internal static string WatchUrl => $"{HomeUrl}watch/";
            internal static string ImageUrl => $"https://static.{BaseDomain}/series/";
            internal static string TestUrl => $"{WatchUrl}1";
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
