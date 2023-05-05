namespace SdarotAPI;

public class SdarotDriver
{
    private readonly HttpClient _httpClient;

    public SdarotDriver() : this(true) { }

    public SdarotDriver(bool doChecks)
    {
        _httpClient = new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        Constants.SdarotUrls.BaseDomain = RetrieveSdarotDomain().Result;
        _httpClient.DefaultRequestHeaders.Referrer = new Uri(Constants.SdarotUrls.HomeUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);

        if (doChecks && !TestConnection().Result)
        {
            throw new SdarotBlockedException();
        }
    }

    private async Task<bool> TestConnection()
    {
        try
        {
            _ = await _httpClient.GetAsync(Constants.SdarotUrls.TestUrl);
        }
        catch (HttpRequestException)
        {
            return false;
        }

        return true;
    }

    public static async Task<string> RetrieveSdarotDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.SdarotUrls.SdarotUrlSource)).Trim();
    }

    public async Task<bool> IsLoggedIn()
    {
        var searchHtml = await _httpClient.GetStringAsync(Constants.SdarotUrls.HomeUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(searchHtml);

        var loginPanelButton = doc.DocumentNode.SelectSingleNode(Constants.XPathSelectors.MainPageLoginPanelButton);

        return loginPanelButton != null ? loginPanelButton.InnerText != Constants.LoginMessage : throw new ElementNotFoundException(nameof(loginPanelButton));
    }

    public async Task<bool> Login(string username, string password)
    {
        Dictionary<string, string> data = new()
        {
            ["username"] = username,
            ["password"] = password,
            ["submit_login"] = ""
        };

        _ = await _httpClient.PostAsync(Constants.SdarotUrls.LoginUrl, new FormUrlEncodedContent(data));

        return await IsLoggedIn();
    }

    public async Task<IEnumerable<ShowInformation>> SearchShow(string searchQuery)
    {
        var showsJson = await _httpClient.GetStringAsync($"{Constants.SdarotUrls.AjaxAllShowsUrl}");
        var shows = JsonSerializer.Deserialize<List<ShowInformation>>(showsJson);

        var relevantShows = shows?.Where(x =>
            x.NameHe.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase) ||
            x.NameEn.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase)) ?? Enumerable.Empty<ShowInformation>();

        return relevantShows;
    }

    public async Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        var showHtml = await _httpClient.GetStringAsync(show.Url);
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
            var seasonNumber = season.GetAttributeValue("data-season", 0);
            var seasonName = season.SelectSingleNode("a").InnerText;
            seasons.Add(new(seasonNumber, i, seasonName, show));
        }

        return seasons;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        var episodesHtml = await _httpClient.GetStringAsync($"{Constants.SdarotUrls.AjaxWatchUrl}?episodeList={season.Show.Code}&season={season.Number}");
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
            var episodeNumber = episode.GetAttributeValue("data-episode", 0);
            var episodeName = episode.SelectSingleNode("a").InnerText;
            episodes.Add(new(episodeNumber, i, episodeName, season));
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode, int maxEpisodeAmount)
    {
        var episodesBuffer = new Queue<EpisodeInformation>((await GetEpisodesAsync(firstEpisode.Season)).ToArray()[firstEpisode.Index..]);
        var seasonBuffer = new Queue<SeasonInformation>((await GetSeasonsAsync(firstEpisode.Season.Show)).ToArray()[(firstEpisode.Season.Index + 1)..]);

        List<EpisodeInformation> episodes = new();
        while (episodes.Count < maxEpisodeAmount)
        {
            if (episodesBuffer.Count == 0)
            {
                if (seasonBuffer.Count == 0)
                {
                    break;
                }

                episodesBuffer = new(await GetEpisodesAsync(seasonBuffer.Dequeue()));
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

    public async Task<string> GetEpisodeMediaUrlAsync(EpisodeInformation episode) => await GetEpisodeMediaUrlAsync(episode, null);

    public async Task<string> GetEpisodeMediaUrlAsync(EpisodeInformation episode, IProgress<double>? progress)
    {
        Dictionary<string, string> data = new()
        {
            ["preWatch"] = true.ToString(),
            ["SID"] = episode.Show.Code.ToString(),
            ["season"] = episode.Season.Number.ToString(),
            ["ep"] = episode.Number.ToString()
        };

        var res = await _httpClient.PostAsync(Constants.SdarotUrls.AjaxWatchUrl, new FormUrlEncodedContent(data));
        var token = await res.Content.ReadAsStringAsync();

        for (var i = 0.1; i <= 30; i += 0.1)
        {
            await Task.Delay(100);
            progress?.Report(i);
        }

        data = new()
        {
            ["watch"] = false.ToString(),
            ["token"] = token,
            ["serie"] = episode.Show.Code.ToString(),
            ["season"] = episode.Season.Number.ToString(),
            ["episode"] = episode.Number.ToString(),
            ["type"] = "episode"
        };

        res = await _httpClient.PostAsync(Constants.SdarotUrls.AjaxWatchUrl, new FormUrlEncodedContent(data));

        var watchResult = await res.Content.ReadFromJsonAsync<WatchResult>() ?? throw new WebsiteErrorException("Unable to retrieve episode media url.");
        var bestResolution = watchResult.Watch.Max((res) => res.Key);

        return watchResult.Watch[bestResolution];
    }

    public async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream) => await DownloadEpisodeAsync(episodeMediaUrl, stream, null);
    public async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream, IProgress<long>? progress) => await DownloadEpisodeAsync(episodeMediaUrl, stream, progress, default);
    public async Task DownloadEpisodeAsync(string episodeMediaUrl, Stream stream, IProgress<long>? progress, CancellationToken ct) => await _httpClient.DownloadAsync(episodeMediaUrl, stream, progress, ct);
}
