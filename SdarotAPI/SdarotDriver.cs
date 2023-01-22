namespace SdarotAPI;

public class SdarotDriver
{
    private readonly HttpClient _httpClient = new();

    public SdarotDriver() : this(false) { }

    public static async Task<string> RetrieveSdarotDomain()
    {
        using HttpClient client = new();
        return (await client.GetStringAsync(Constants.SdarotUrls.SdarotUrlSource)).Trim();
    }

    public SdarotDriver(bool ignoreChecks)
    {
        Constants.SdarotUrls.BaseDomain = RetrieveSdarotDomain().Result;
        _httpClient.DefaultRequestHeaders.Referrer = new Uri(Constants.SdarotUrls.HomeUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);

        if (!ignoreChecks)
        {
            try
            {
                _httpClient.GetAsync(Constants.SdarotUrls.TestUrl).Wait();
            }
            catch
            {
                throw new SdarotBlockedException();
            }
        }
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

    public async Task<IEnumerable<SeriesInformation>> SearchSeries(string searchQuery)
    {
        var searchHtml = await _httpClient.GetStringAsync($"{Constants.SdarotUrls.SearchUrl}{searchQuery}");
        var doc = new HtmlDocument();
        doc.LoadHtml(searchHtml);

        var seriesNameElement = doc.DocumentNode.SelectSingleNode(Constants.XPathSelectors.SeriesPageSeriesName);

        // In case there is only one result
        if (seriesNameElement is not null)
        {
            var seriesName = seriesNameElement.InnerText.Trim(new[] { ' ', '/' });

            var imageUrlElement = doc.DocumentNode.SelectSingleNode(Constants.XPathSelectors.SeriesPageSeriesImage);
            if (imageUrlElement is null)
            {
                throw new ElementNotFoundException(nameof(imageUrlElement));
            }

            var imageUrl = imageUrlElement.GetAttributeValue("src", "");

            return new SeriesInformation(HttpUtility.HtmlDecode(seriesName), imageUrl).Yield();
        }

        var seriesElements = doc.DocumentNode.SelectNodes(Constants.XPathSelectors.SearchPageResult);

        // In case there are no results
        if (seriesElements is null || seriesElements.Count == 0)
        {
            return Enumerable.Empty<SeriesInformation>();
        }

        // In case there are more than one result

        var seriesList = new List<SeriesInformation>();
        foreach (var seriesElement in seriesElements)
        {
            var seriesNameHe = seriesElement.SelectSingleNode(Constants.XPathSelectors.SearchPageResultInnerSeriesNameHe).InnerText;
            var seriesNameEn = seriesElement.SelectSingleNode(Constants.XPathSelectors.SearchPageResultInnerSeriesNameEn).InnerText;
            var imageUrl = seriesElement.SelectSingleNode("img").GetAttributeValue("src", "");
            seriesList.Add(new(HttpUtility.HtmlDecode(seriesNameHe), HttpUtility.HtmlDecode(seriesNameEn), imageUrl));
        }

        return seriesList;
    }

    public async Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(SeriesInformation series)
    {
        var seriesHtml = await _httpClient.GetStringAsync(series.SeriesUrl);
        var doc = new HtmlDocument();
        doc.LoadHtml(seriesHtml);

        var seasonElements = doc.DocumentNode.SelectNodes(Constants.XPathSelectors.SeriesPageSeason);

        if (seasonElements is null || seasonElements.Count == 0)
        {
            return Enumerable.Empty<SeasonInformation>();
        }

        List<SeasonInformation> seasons = new();
        foreach (var (season, i) in seasonElements.Select((season, i) => (season, i)))
        {
            var seasonNumber = season.GetAttributeValue("data-season", 0);
            var seasonName = season.SelectSingleNode("a").InnerText;
            seasons.Add(new(seasonNumber, i, seasonName, series));
        }

        return seasons;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        var episodesHtml = await _httpClient.GetStringAsync($"{Constants.SdarotUrls.AjaxWatchUrl}?episodeList={season.Series.SeriesCode}&season={season.SeasonNumber}");
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
        var episodesBuffer = new Queue<EpisodeInformation>((await GetEpisodesAsync(firstEpisode.Season)).ToArray()[firstEpisode.EpisodeIndex..]);
        var seasonBuffer = new Queue<SeasonInformation>((await GetSeasonsAsync(firstEpisode.Season.Series)).ToArray()[(firstEpisode.Season.SeasonIndex + 1)..]);

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

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeriesInformation series)
    {
        var seasons = await GetSeasonsAsync(series);

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
            ["SID"] = episode.Series.SeriesCode.ToString(),
            ["season"] = episode.Season.SeasonNumber.ToString(),
            ["ep"] = episode.EpisodeNumber.ToString()
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
            ["serie"] = episode.Series.SeriesCode.ToString(),
            ["season"] = episode.Season.SeasonNumber.ToString(),
            ["episode"] = episode.EpisodeNumber.ToString(),
            ["type"] = "episode"
        };

        res = await _httpClient.PostAsync(Constants.SdarotUrls.AjaxWatchUrl, new FormUrlEncodedContent(data));

        var watchResult = await res.Content.ReadFromJsonAsync<WatchResult>();

        if (watchResult is null)
            throw new WebsiteErrorException("Unable to retrieve episode media url.");

        var bestResolution = watchResult.Watch.Max((res) => res.Key);

        return watchResult.Watch[bestResolution];
    }

    public async Task DownloadEpisode(string episodeMediaUrl, Stream stream) => await DownloadEpisode(episodeMediaUrl, stream, null);
    public async Task DownloadEpisode(string episodeMediaUrl, Stream stream, IProgress<long>? progress) => await DownloadEpisode(episodeMediaUrl, stream, progress, default);

    public async Task DownloadEpisode(string episodeMediaUrl, Stream stream, IProgress<long>? progress, CancellationToken ct) => await _httpClient.DownloadAsync(episodeMediaUrl, stream, progress, ct);
}