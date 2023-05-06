namespace SdarotAPI;

public class SdarotDriver
{
    private readonly HttpClient httpClient;

    public SdarotDriver() : this(true)
    {
    }

    public SdarotDriver(bool doChecks)
    {
        httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        Constants.SdarotUrls.BaseDomain = RetrieveSdarotDomain().Result;
        httpClient.DefaultRequestHeaders.Referrer = new Uri(Constants.SdarotUrls.HomeUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);

        if (doChecks && !TestConnection().Result)
        {
            throw new SdarotBlockedException();
        }
    }

    private async Task<bool> TestConnection()
    {
        try
        {
            _ = await httpClient.GetAsync(Constants.SdarotUrls.TestUrl);
        }
        catch (HttpRequestException)
        {
            return false;
        }

        return true;
    }

    private static async Task<string> RetrieveSdarotDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.SdarotUrls.SdarotUrlSource)).Trim();
    }

    public async Task<bool> IsLoggedIn()
    {
        string searchHtml = await httpClient.GetStringAsync(Constants.SdarotUrls.HomeUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(searchHtml);

        var loginPanelButton = doc.DocumentNode.SelectSingleNode(Constants.XPathSelectors.MainPageLoginPanelButton);

        return loginPanelButton != null
            ? loginPanelButton.InnerText != Constants.LoginMessage
            : throw new ElementNotFoundException(nameof(loginPanelButton));
    }

    public async Task<bool> Login(string username, string password)
    {
        Dictionary<string, string> data = new()
        {
            ["username"] = username,
            ["password"] = password,
            ["submit_login"] = ""
        };

        _ = await httpClient.PostAsync(Constants.SdarotUrls.LoginUrl, new FormUrlEncodedContent(data));

        return await IsLoggedIn();
    }

    public async Task<IEnumerable<ShowInformation>> SearchShow(string searchQuery)
    {
        var shows = await JsonSerializer.DeserializeAsync<List<ShowInformation>>(
            await httpClient.GetStreamAsync(Constants.SdarotUrls.AjaxAllShowsUrl));

        var relevantShows = shows?.Where(x =>
                                x.NameHe.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) ||
                                x.NameEn.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase)) ??
                            Enumerable.Empty<ShowInformation>();

        return relevantShows;
    }

    public async Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        string showHtml = await httpClient.GetStringAsync(show.Url);
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
            int seasonNumber = season.GetAttributeValue("data-season", 0);
            string? seasonName = season.SelectSingleNode("a").InnerText;
            seasons.Add(new SeasonInformation(seasonName, seasonNumber, i, show));
        }

        return seasons;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        string episodesHtml =
            await httpClient.GetStringAsync(
                $"{Constants.SdarotUrls.AjaxWatchUrl}?episodeList={season.Show.Code}&season={season.Number}");
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
            int episodeNumber = episode.GetAttributeValue("data-episode", 0);
            string? episodeName = episode.SelectSingleNode("a").InnerText;
            episodes.Add(new EpisodeInformation(episodeName, episodeNumber, i, season));
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode,
        int maxEpisodeAmount)
    {
        var episodesBuffer =
            new Queue<EpisodeInformation>(
                (await GetEpisodesAsync(firstEpisode.Season)).ToArray()[firstEpisode.Index..]);
        var seasonBuffer =
            new Queue<SeasonInformation>(
                (await GetSeasonsAsync(firstEpisode.Season.Show)).ToArray()[(firstEpisode.Season.Index + 1)..]);

        List<EpisodeInformation> episodes = new();
        while (episodes.Count < maxEpisodeAmount)
        {
            if (episodesBuffer.Count == 0)
            {
                if (seasonBuffer.Count == 0)
                {
                    break;
                }

                episodesBuffer = new Queue<EpisodeInformation>(await GetEpisodesAsync(seasonBuffer.Dequeue()));
                continue;
            }

            episodes.Add(episodesBuffer.Dequeue());
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show)
    {
        var seasons = await GetSeasonsAsync(show);

        List<EpisodeInformation> episodes = new();
        foreach (var season in seasons)
        {
            episodes.AddRange(await GetEpisodesAsync(season));
        }

        return episodes;
    }

    public async Task<string> GetEpisodeMediaUrlAsync(EpisodeInformation episode) =>
        await GetEpisodeMediaUrlAsync(episode, null);

    private async Task<string> GetEpisodeMediaUrlAsync(EpisodeInformation episode, IProgress<double>? progress)
    {
        Dictionary<string, string> data = new()
        {
            ["preWatch"] = true.ToString(),
            ["SID"] = episode.Show.Code.ToString(),
            ["season"] = episode.Season.Number.ToString(),
            ["ep"] = episode.Number.ToString()
        };

        var postResult = await httpClient.PostAsync(Constants.SdarotUrls.AjaxWatchUrl, new FormUrlEncodedContent(data));
        string token = await postResult.Content.ReadAsStringAsync();

        for (double i = 0.1; i <= 30; i += 0.1)
        {
            await Task.Delay(100);
            progress?.Report(i);
        }

        data = new Dictionary<string, string>
        {
            ["watch"] = false.ToString(),
            ["token"] = token,
            ["serie"] = episode.Show.Code.ToString(),
            ["season"] = episode.Season.Number.ToString(),
            ["episode"] = episode.Number.ToString(),
            ["type"] = "episode"
        };

        postResult = await httpClient.PostAsync(Constants.SdarotUrls.AjaxWatchUrl, new FormUrlEncodedContent(data));

        var media = await postResult.Content.ReadFromJsonAsync<Media>() ??
                    throw new WebsiteErrorException("Unable to retrieve episode media url.");

        return media.MaxResolutionLink;
    }

    public async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream) =>
        await DownloadEpisodeAsync(episodeMediaUrl, stream, null);

    private async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream, IProgress<long>? progress) =>
        await DownloadEpisodeAsync(episodeMediaUrl, stream, progress, default(CancellationToken));

    private async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream, IProgress<long>? progress,
        CancellationToken ct) => await httpClient.DownloadAsync(episodeMediaUrl, stream, progress, ct);
}
