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

        _ = new SdarotDriver(true);

        sw.Stop();

        Trace.WriteLine($"Driver initialization: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task SearchTest()
    {
        SdarotDriver driver = new(true);

        Assert.AreEqual(0, (await driver.SearchSeries("dsakdjaslkfjsalkjfas")).Count());
        Assert.IsTrue((await driver.SearchSeries("שמש")).Count() > 0);
        Assert.IsTrue((await driver.SearchSeries("ana")).Count() > 0);
        Assert.AreEqual(1, (await driver.SearchSeries("shemesh")).Count());
    }

    [TestMethod]
    public async Task SearchTestBenchmark()
    {
        SdarotDriver driver = new(true);

        Trace.WriteLine($"No results: {(await MeasureSearch(driver, "dsakdjaslkfjsalkjfas")).TotalSeconds} seconds.");
        Trace.WriteLine($"Few results: {(await MeasureSearch(driver, "שמש")).TotalSeconds} seconds.");
        Trace.WriteLine($"Much results: {(await MeasureSearch(driver, "ana")).TotalSeconds} seconds.");
        Trace.WriteLine($"One result: {(await MeasureSearch(driver, "shemesh")).TotalSeconds} seconds.");
    }

    public static async Task<TimeSpan> MeasureSearch(SdarotDriver driver, string query)
    {
        await Task.Delay(500);
        Stopwatch sw = new();
        sw.Start();

        _ = await driver.SearchSeries(query);

        sw.Stop();
        return sw.Elapsed;
    }

    [TestMethod]
    public async Task SeasonsTest()
    {
        await Task.Delay(500);
        SdarotDriver driver = new(true);

        SeriesInformation series = new("איש משפחה / Family Guy", "static.sdarot.tw/series/1.jpg");

        Stopwatch sw = new();
        sw.Start();

        _ = await driver.GetSeasonsAsync(series);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task EpisodesTest()
    {
        await Task.Delay(500);
        SdarotDriver driver = new(true);

        SeriesInformation series = new("איש משפחה / Family Guy", "static.sdarot.tw/series/1.jpg");
        SeasonInformation season = new(4, 3, "4", series);

        Stopwatch sw = new();
        sw.Start();

        _ = await driver.GetEpisodesAsync(season);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }
}