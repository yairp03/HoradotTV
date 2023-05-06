namespace SdarotAPITest;

[TestClass]
public class ApiUnitTest
{
    [TestMethod]
    public void DriverInitTest()
    {
        Stopwatch sw = new();
        sw.Start();

        _ = new SdarotDriver(false);

        sw.Stop();

        Trace.WriteLine($"Driver initialization: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task SearchTest()
    {
        SdarotDriver driver = new(false);

        Assert.AreEqual(0, (await driver.SearchShow("dsakdjaslkfjsalkjfas")).Count());
        Assert.IsTrue((await driver.SearchShow("שמש")).Any());
        Assert.IsTrue((await driver.SearchShow("ana")).Any());
        Assert.AreEqual(1, (await driver.SearchShow("shemesh")).Count());
    }

    [TestMethod]
    public async Task SearchTestBenchmark()
    {
        SdarotDriver driver = new(false);

        Trace.WriteLine($"No results: {(await MeasureSearch(driver, "dsakdjaslkfjsalkjfas")).TotalSeconds} seconds.");
        Trace.WriteLine($"Few results: {(await MeasureSearch(driver, "שמש")).TotalSeconds} seconds.");
        Trace.WriteLine($"Much results: {(await MeasureSearch(driver, "ana")).TotalSeconds} seconds.");
        Trace.WriteLine($"One result: {(await MeasureSearch(driver, "shemesh")).TotalSeconds} seconds.");
    }

    private static async Task<TimeSpan> MeasureSearch(SdarotDriver driver, string query)
    {
        Stopwatch sw = new();
        sw.Start();

        _ = await driver.SearchShow(query);

        sw.Stop();
        return sw.Elapsed;
    }

    [TestMethod]
    public async Task SeasonsTest()
    {
        SdarotDriver driver = new(false);

        ShowInformation show = new("איש משפחה", "Family Guy", 1);

        Stopwatch sw = new();
        sw.Start();

        _ = await driver.GetSeasonsAsync(show);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task EpisodesTest()
    {
        SdarotDriver driver = new(false);

        ShowInformation show = new("איש משפחה", "Family Guy", 1);
        SeasonInformation season = new("4", 4, 3, show);

        Stopwatch sw = new();
        sw.Start();

        _ = await driver.GetEpisodesAsync(season);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }
}
