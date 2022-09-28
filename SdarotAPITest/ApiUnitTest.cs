namespace SdarotAPITest;

[TestClass]
public class ApiUnitTest
{
    [TestMethod]
    public async Task DriverInitTest()
    {
        await Task.Delay(500);
        Stopwatch sw = new();
        sw.Start();

        SdarotDriver driver = new(ignoreChecks: true);

        sw.Stop();

        Trace.WriteLine($"Driver initialization: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task SearchTest()
    {
        SdarotDriver driver = new(ignoreChecks: true);

        Trace.WriteLine($"No results: {(await MeasureSearch(driver, "dsakdjaslkfjsalkjfas")).TotalSeconds} seconds.");
        Trace.WriteLine($"15 results: {(await MeasureSearch(driver, "שמש")).TotalSeconds} seconds.");
        Trace.WriteLine($"74 results: {(await MeasureSearch(driver, "ana")).TotalSeconds} seconds.");
        Trace.WriteLine($"One result: {(await MeasureSearch(driver, "shemesh")).TotalSeconds} seconds.");
    }

    public static async Task<TimeSpan> MeasureSearch(SdarotDriver driver, string query)
    {
        await Task.Delay(500);
        Stopwatch sw = new();
        sw.Start();

        await driver.SearchSeries(query);

        sw.Stop();
        return sw.Elapsed;
    }

    [TestMethod]
    public async Task SeasonsTest()
    {
        await Task.Delay(500);
        SdarotDriver driver = new(ignoreChecks: true);

        var series = (await driver.SearchSeries("family guy")).ToList()[0];

        Stopwatch sw = new();
        sw.Start();

        await driver.GetSeasonsAsync(series);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task EpisodesTest()
    {
        await Task.Delay(500);
        SdarotDriver driver = new(ignoreChecks: true);

        var series = (await driver.SearchSeries("family guy")).ToList()[0];
        var season = (await driver.GetSeasonsAsync(series)).ToList()[3];

        Stopwatch sw = new();
        sw.Start();

        await driver.GetEpisodesAsync(season);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }
}