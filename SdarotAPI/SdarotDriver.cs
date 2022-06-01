using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SdarotAPI.Exceptions;
using SdarotAPI.Model;
using SdarotAPI.Resources;
using SeleniumExtras.WaitHelpers;
using System.Collections.ObjectModel;
using System.Net;
using Cookie = System.Net.Cookie;

namespace SdarotAPI;

public class SdarotDriver
{
    ChromeDriver? webDriver;

    public SdarotDriver()
    {
    }

    public async Task Initialize(bool headless = true)
    {
        var options = new ChromeOptions();
        if (headless)
        {
            options.AddArgument("headless");
        }
        webDriver = new ChromeDriver(options);

        Constants.SdarotUrls.BaseDomain = await SdarotHelper.RetrieveSdarotDomain();

        try
        {
            webDriver.Navigate().GoToUrl(Constants.SdarotUrls.TestUrl);
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

    public async Task NavigateAsync(string url)
    {
        await Task.Run(() => webDriver!.Navigate().GoToUrl(url));
    }

    public async Task NavigateToSeriesAsync(SeriesInformation series)
    {
        await NavigateAsync(series.SeriesUrl);
    }

    public async Task NavigateToSeasonAsync(SeasonInformation season)
    {
        await NavigateAsync(season.SeasonUrl);
    }

    public async Task NavigateToEpisodeAsync(EpisodeInformation episode)
    {
        await NavigateAsync(episode.EpisodeUrl);
    }

    public async Task<IWebElement> FindElementAsync(By by, int timeout = 10)
    {
        return await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementIsVisible(by)));
    }

    public static async Task<IWebElement> FindElementAsync(By by, ISearchContext context)
    {
        return await Task.Run(() => context.FindElement(by));
    }

    public async Task<IWebElement> FindClickableElementAsync(By by, int timeout = 10)
    {
        return await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementToBeClickable(by)));
    }

    public async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, int timeout = 10)
    {
        return await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(by)));
    }

    public static async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, ISearchContext context)
    {
        return await Task.Run(() => context.FindElements(by));
    }

    public CookieContainer RetrieveCookies()
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
        webDriver?.Quit();
    }

    public async Task<SeriesInformation[]> SearchSeries(string searchQuery)
    {
        if (webDriver is null)
        {
            throw new DriverNotInitializedException();
        }
        await NavigateAsync($"{Constants.SdarotUrls.SearchUrl}{searchQuery}");

        // In case there is only one result
        if (webDriver.Url.StartsWith(Constants.SdarotUrls.WatchUrl))
        {
            string seriesName = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeriesName))).Text.Trim(new char[]  { ' ', '/' });
            string imageUrl = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeriesImage))).GetAttribute("src");
            return new SeriesInformation[] { new(seriesName, imageUrl) };
        }
        
        var results = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SearchPageResult));

        // In case there are no results
        if (results.Count == 0)
        {
            return Array.Empty<SeriesInformation>();
        }

        // In case there are more than one result
        var seriesList = new List<SeriesInformation>();
        foreach (var result in results)
        {
            string seriesNameHe = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SearchPageResultInnerSeriesNameHe), result)).Text;
            string seriesNameEn = (await FindElementAsync(By.XPath(Constants.XPathSelectors.SearchPageResultInnerSeriesNameEn), result)).Text;
            string imageUrl = (await FindElementAsync(By.TagName("img"), result)).GetAttribute("src");
            seriesList.Add(new(seriesNameHe, seriesNameEn, imageUrl));
        }

        return seriesList.ToArray();
    }

    public async Task<SeasonInformation[]> GetSeasonsAsync(SeriesInformation series)
    {
        await NavigateToSeriesAsync(series);

        var seasonElements = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SeriesPageSeason));

        List<SeasonInformation> seasons = new();
        for (int i = 0; i < seasonElements.Count; i++)
        {
            var element = seasonElements[i];
            int seasonNumber = int.Parse(element.GetAttribute("data-season"));
            string seasonName = (await FindElementAsync(By.TagName("a"), element)).Text;
            seasons.Add(new(seasonNumber, i, seasonName, series));
        }

        return seasons.ToArray();
    }

    public async Task<EpisodeInformation[]> GetEpisodesAsync(SeasonInformation season)
    {
        await NavigateToSeasonAsync(season);

        var episodeElements = await FindElementsAsync(By.XPath(Constants.XPathSelectors.SeriesPageEpisode));

        List<EpisodeInformation> episodes = new();
        for (int i = 0; i < episodeElements.Count; i++)
        {
            var element = episodeElements[i];
            int episodeNumber = int.Parse(element.GetAttribute("data-episode"));
            string episodeName = (await FindElementAsync(By.TagName("a"), element)).Text;
            episodes.Add(new(episodeNumber, i, episodeName, season));
        }

        return episodes.ToArray();
    }

    public async Task<EpisodeInformation[]> GetEpisodesAsync(EpisodeInformation firstEpisode, int maxEpisodeAmount)
    {
        var episodesBuffer = new Queue<EpisodeInformation>((await GetEpisodesAsync(firstEpisode.Season))[firstEpisode.EpisodeIndex..]);
        var seasonBuffer = new Queue<SeasonInformation>((await GetSeasonsAsync(firstEpisode.Season.Series))[(firstEpisode.Season.SeasonIndex + 1)..]);

        List<EpisodeInformation> episodes = new();
        for (int i = 0; i < maxEpisodeAmount; i++)
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

       return episodes.ToArray();
    }

    public async Task<EpisodeInformation[]> GetEpisodesAsync(SeriesInformation series)
    {
        var seasons = await GetSeasonsAsync(series);

        List<EpisodeInformation> episodes = new();
        foreach (var season in seasons)
        {
            episodes.AddRange(await GetEpisodesAsync(season));
        }

        return episodes.ToArray();
    }

    public async Task<EpisodeMediaDetails?> GetEpisodeMediaDetailsAsync(EpisodeInformation episode, IProgress<float>? progress = null)
    {
        await NavigateToEpisodeAsync(episode);

        // Wait for button to show up
        float currSeconds = Constants.WaitTime;
        while (currSeconds > 0)
        {
            float newSeconds = float.Parse((await FindElementAsync(By.XPath(Constants.XPathSelectors.SeriesPageEpisodeWaitTime))).Text);
            if (newSeconds != currSeconds)
            {
                currSeconds = newSeconds;
                if (progress != null)
                {
                    progress.Report(currSeconds);
                }
            }
        }

        // Click button
        (await FindClickableElementAsync(By.Id(Constants.IdSelectors.ProceedButtonId))).Click();

        string mediaUrl = (await FindElementAsync(By.Id(Constants.IdSelectors.EpisodeMedia))).GetAttribute("src");
        CookieContainer cookies = RetrieveCookies();

        return new EpisodeMediaDetails(mediaUrl, cookies);
    }
}