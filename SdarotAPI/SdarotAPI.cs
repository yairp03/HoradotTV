using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using SdarotAPI.Exceptions;
using SdarotAPI.Model;
using SdarotAPI.Resources;

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

        webDriver.Navigate().GoToUrl(Constants.WatchUrl);

        if (webDriver.Title == "Privacy error")
        {
            throw new SdarotBlockedException();
        }
    }

    public void Shutdown()
    {
        webDriver?.Quit();
    }

    public SeriesInformation[] SearchSeries(string searchQuery)
    {
        if (webDriver is null)
        {
            throw new DriverNotInitializedException();
        }
        webDriver.Navigate().GoToUrl($"{Constants.SearchUrl}{searchQuery}");

        // In case there is only one result
        if (webDriver.Url.StartsWith(Constants.WatchUrl))
        {
            string seriesName = webDriver.FindElement(By.XPath("//*[@id=\"watchEpisode\"]/div[1]/div/h1/strong")).Text.Trim(new char[]  { ' ', '/' });
            string imageUrl = webDriver.FindElement(By.XPath("//*[@id=\"watchEpisode\"]/div[2]/div/div[1]/div[1]/img")).GetAttribute("src");
            return new SeriesInformation[] { new(seriesName, imageUrl) };
        }
        
        var results = webDriver.FindElements(By.CssSelector("div.col-lg-2.col-md-2.col-sm-4.col-xs-6"));

        // In case there are no results
        if (results.Count == 0)
        {
            return new SeriesInformation[0];
        }

        // In case there are more than one result
        throw new NotImplementedException();
    }
}