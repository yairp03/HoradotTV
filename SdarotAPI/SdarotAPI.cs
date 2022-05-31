using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SdarotAPI.Exceptions;
using SdarotAPI.Model;
using SdarotAPI.Resources;
using System.Collections.ObjectModel;

namespace SdarotAPI;

public class SdarotDriver
{
    ChromeDriver? webDriver;

    public SdarotDriver()
    {
    }

    public void Initialize(bool headless = true)
    {
        var options = new ChromeOptions();
        if (headless)
        {
            options.AddArgument("headless");
        }
        webDriver = new ChromeDriver(options);

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
        await NavigateAsync($"{Constants.SdarotUrls.WatchUrl}{series.SeriesCode}");
    }

    public async Task<IWebElement> FindElementAsync(By by, int timeout = 10)
    {
        return await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(e => e.FindElement(by)));
    }

    public async Task<IWebElement> FindElementAsync(By by, ISearchContext context)
    {
        return await Task.Run(() => context.FindElement(by));
    }

    public async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, int timeout = 10)
    {
        return await Task.Run(() => new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeout)).Until(e => e.FindElements(by)));
    }

    public async Task<ReadOnlyCollection<IWebElement>> FindElementsAsync(By by, ISearchContext context)
    {
        return await Task.Run(() => context.FindElements(by));
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

        foreach (var element in seasonElements)
        {
            int seasonNumber = int.Parse(element.GetAttribute("data-season"));
            string seasonName = (await FindElementAsync(By.TagName("a"), element)).Text;
            seasons.Add(new(seasonNumber, seasonName, series));
        }

        return seasons.ToArray();
    }
}