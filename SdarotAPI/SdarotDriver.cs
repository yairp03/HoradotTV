namespace SdarotAPI;

public class SdarotDriver
{
    ChromeDriver? webDriver;

    public bool IsInitialized => webDriver is not null;

    public SdarotDriver()
    {
    }

    public async Task Initialize(bool headless = true)
    {
        if (IsInitialized)
        {
            throw new DriverAlreadyInitializedException();
        }

        await ChromeDriverHelper.Install();

        var driverService = ChromeDriverService.CreateDefaultService();
        driverService.HideCommandPromptWindow = true;
        ChromeOptions options = new();
        options.AddArgument("user-agent=" + Constants.UserAgent);
        if (headless)
        {
            options.AddArgument("headless");
            options.AddArgument("--remote-debugging-port=9222");
        }

        webDriver = new ChromeDriver(driverService, options);

        Constants.SdarotUrls.BaseDomain = await SdarotHelper.RetrieveSdarotDomain();

        try
        {
            await NavigateAsync(Constants.SdarotUrls.TestUrl);
        }
        catch (WebDriverException)
        {
            throw new SdarotBlockedException();
        }

        if (webDriver.Title == "Privacy error")
        {
            throw new SdarotBlockedException();
        }
    }

    public async Task<bool> IsLoggedIn()
    {
        await NavigateAsync(Constants.SdarotUrls.HomeUrl);
        var loginPanelButton = await FindElementAsync(By.XPath(Constants.XPathSelectors.MainPageLoginPanelButton));
        return loginPanelButton.Text != Constants.LoginMessage;
    }

    public async Task<bool> Login(string username, string password)
    {
        if (await IsLoggedIn())
            return true;

        var loginPanelButton = await FindElementAsync(By.XPath(Constants.XPathSelectors.MainPageLoginPanelButton));
        loginPanelButton.Click();
        var usernameInput = await FindElementAsync(By.XPath(Constants.XPathSelectors.MainPageFormUsername));
        var passwordInput = await FindElementAsync(By.XPath(Constants.XPathSelectors.MainPageFormPassword));
        usernameInput.SendKeys(username);
        passwordInput.SendKeys(password);
        var loginButton = await FindElementAsync(By.XPath(Constants.XPathSelectors.MainPageLoginButton));
        await Task.Delay(1000);
        loginButton.Click();

        return await IsLoggedIn();
    }

    async Task NavigateAsync(string url) => await Task.Run(() => webDriver!.Navigate().GoToUrl(url));

    async Task NavigateToSeriesAsync(SeriesInformation series) => await NavigateAsync(series.SeriesUrl);

    async Task NavigateToSeasonAsync(SeasonInformation season) => await NavigateAsync(season.SeasonUrl);

    async Task NavigateToEpisodeAsync(EpisodeInformation episode) => await NavigateAsync(episode.EpisodeUrl);

    async Task<IWebElement> FindElementAsync(By by, int timeout = 2) => await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementIsVisible(by)));

    static async Task<IWebElement> FindElementAsync(By by, ISearchContext context) => await Task.Run(() => context.FindElement(by));

    async Task<IWebElement> FindClickableElementAsync(By by, int timeout = 2) => await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementToBeClickable(by)));

    async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, int timeout = 2) => await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(by)));

    static async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, ISearchContext context) => await Task.Run(() => context.FindElements(by));

    CookieContainer RetrieveCookies()
    {
        CookieContainer cookies = new();
        foreach (var cookie in webDriver!.Manage().Cookies.AllCookies)
        {
            cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
        }

        return cookies;
    }

    public void Shutdown()
    {
        if (IsInitialized)
        {
            webDriver?.Quit();
        }
    }

    public async Task<IEnumerable<SeriesInformation>> SearchSeries(string searchQuery)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        await NavigateAsync($"{Constants.SdarotUrls.SearchUrl}{searchQuery}");

        // In case there is only one result
        if (webDriver!.Url.StartsWith(Constants.SdarotUrls.WatchUrl))
        {
            var seriesName = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeriesName))).Text.Trim(new char[] { ' ', '/' });
            var imageUrl = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeriesImage))).GetAttribute("src");
            return new SeriesInformation[] { new(seriesName, imageUrl) };
        }

        ReadOnlyCollection<IWebElement>? results = null;
        try
        {
            results = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SearchPageResult));
        }
        catch (WebDriverTimeoutException) { }

        // In case there are no results
        if (results is null || results.Count == 0)
        {
            return Enumerable.Empty<SeriesInformation>();
        }

        // In case there are more than one result
        var seriesList = new List<SeriesInformation>();
        foreach (var result in results)
        {
            var seriesNameHe = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SearchPageResultInnerSeriesNameHe), result)).GetAttribute("innerText");
            var seriesNameEn = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SearchPageResultInnerSeriesNameEn), result)).GetAttribute("innerText");
            var imageUrl = (await FindElementAsync(By.TagName("img"), result)).GetAttribute("src");
            seriesList.Add(new(seriesNameHe, seriesNameEn, imageUrl));
        }

        return seriesList;
    }

    public async Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(SeriesInformation series)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        await NavigateToSeriesAsync(series);

        var seasonElements = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeason));

        List<SeasonInformation> seasons = new();
        for (var i = 0; i < seasonElements.Count; i++)
        {
            var element = seasonElements[i];
            var seasonNumber = int.Parse(element.GetAttribute("data-season"));
            var seasonName = (await FindElementAsync(By.TagName("a"), element)).Text;
            seasons.Add(new(seasonNumber, i, seasonName, series));
        }

        return seasons;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        await NavigateToSeasonAsync(season);

        var episodeElements = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SeriesPageEpisode));

        List<EpisodeInformation> episodes = new();
        for (var i = 0; i < episodeElements.Count; i++)
        {
            var element = episodeElements[i];
            var episodeNumber = int.Parse(element.GetAttribute("data-episode"));
            var episodeName = (await FindElementAsync(By.TagName("a"), element)).Text;
            episodes.Add(new(episodeNumber, i, episodeName, season));
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode, int maxEpisodeAmount)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        var episodesBuffer = new Queue<EpisodeInformation>((await GetEpisodesAsync(firstEpisode.Season)).ToArray()[firstEpisode.EpisodeIndex..]);
        var seasonBuffer = new Queue<SeasonInformation>((await GetSeasonsAsync(firstEpisode.Season.Series)).ToArray()[(firstEpisode.Season.SeasonIndex + 1)..]);

        List<EpisodeInformation> episodes = new();
        for (var i = 0; i < maxEpisodeAmount; i++)
        {
            if (episodesBuffer.Count == 0)
            {
                if (seasonBuffer.Count == 0)
                {
                    break;
                }

                episodesBuffer = new(await GetEpisodesAsync(seasonBuffer.Dequeue()));
                i--;
                continue;
            }

            episodes.Add(episodesBuffer.Dequeue());
        }

        return episodes;
    }

    public async Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeriesInformation series)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        var seasons = await GetSeasonsAsync(series);

        List<EpisodeInformation> episodes = new();
        foreach (var season in seasons)
        {
            episodes.AddRange(await GetEpisodesAsync(season));
        }

        return episodes;
    }

    public async Task<EpisodeMediaDetails> GetEpisodeMediaDetailsAsync(EpisodeInformation episode, IProgress<float>? progress = null)
    {
        if (!IsInitialized)
        {
            throw new DriverNotInitializedException();
        }

        await NavigateToEpisodeAsync(episode);

        // Wait for button to show up
        var currSeconds = (float)Constants.WaitTime;
        while (currSeconds > 0)
        {
            var newSeconds = float.Parse((await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageEpisodeWaitTime))).Text);
            if (newSeconds != currSeconds)
            {
                currSeconds = newSeconds;
                progress?.Report(30 - currSeconds);
            }
        }

        try
        {
            // Click button
            (await FindClickableElementAsync(By.Id(Constants.IdSelectors.ProceedButtonId))).Click();
        }
        catch (WebDriverException)
        {
            var errorMessage = await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageErrorMessage));
            if (errorMessage.Text == Constants.Error2Message)
            {
                throw new Error2Exception();
            }

            throw new WebsiteErrorException();
        }

        var mediaUrl = (await FindElementAsync(By.Id(Constants.IdSelectors.EpisodeMedia))).GetAttribute("src");
        var cookies = RetrieveCookies();

        return new EpisodeMediaDetails(mediaUrl, cookies, episode);
    }
}