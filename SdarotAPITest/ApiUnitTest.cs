using OpenQA.Selenium.Remote;
using System.Diagnostics;

namespace SdarotAPITest;

[TestClass]
public class ApiUnitTest
{
    [TestMethod]
    public async Task DriverTest()
    {
        SdarotDriver driver = new();
        try
        {
            await driver.Initialize();

            var series = (await driver.SearchSeries("hahamama")).ToList();
            var hamhamama = series[0];
            var seasons = (await driver.GetSeasonsAsync(hamhamama)).ToList();
            var episodes = (await driver.GetEpisodesAsync(seasons[0])).ToList();

            Trace.WriteLine("Done.");
        }
        finally
        {
            driver.Shutdown();
        }
    }

    [TestMethod]
    public async Task SearchTest()
    {
        SdarotDriver driver = new();
        try
        {
            await driver.Initialize();

            Trace.WriteLine($"No results: {(await MeasureSearch(driver, "dsakdjaslkfjsalkjfas")).TotalSeconds} seconds.");
            Trace.WriteLine($"15 results: {(await MeasureSearch(driver, "שמש")).TotalSeconds} seconds.");
            Trace.WriteLine($"74 results: {(await MeasureSearch(driver, "ana")).TotalSeconds}  seconds.");
            Trace.WriteLine($"One result: {(await MeasureSearch(driver, "shemesh")).TotalSeconds}  seconds.");
        }
        finally
        {
            driver.Shutdown();
        }
    }

    public static async Task<TimeSpan> MeasureSearch(SdarotDriver driver, string query)
    {
        Stopwatch sw = new();
        sw.Start();

        await driver.SearchSeries(query);

        sw.Stop();
        return sw.Elapsed;
    }
}