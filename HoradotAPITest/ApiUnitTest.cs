using HoradotAPI.Providers.SdarotTV;

namespace HoradotAPITest;

[TestClass]
public class ApiUnitTest
{
    [TestMethod]
    public async Task DriverInitTest()
    {
        Stopwatch sw = new();
        sw.Start();

        HoradotService service = new();
        await service.InitializeAsync(false);

        sw.Stop();

        Trace.WriteLine($"Driver initialization: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task SearchTest()
    {
        HoradotService service = new();
        await service.InitializeAsync(false);

        Assert.AreEqual(0, (await service.SearchAsync("dsakdjaslkfjsalkjfas")).Count());
        Assert.IsTrue((await service.SearchAsync("שמש")).Any());
        Assert.IsTrue((await service.SearchAsync("ana")).Any());
        Assert.AreEqual(1, (await service.SearchAsync("shemesh")).Count());
    }

    [TestMethod]
    public async Task SearchTestBenchmark()
    {
        HoradotService service = new();
        await service.InitializeAsync(false);

        Trace.WriteLine($"No results: {(await MeasureSearch(service, "dsakdjaslkfjsalkjfas")).TotalSeconds} seconds.");
        Trace.WriteLine($"Few results: {(await MeasureSearch(service, "שמש")).TotalSeconds} seconds.");
        Trace.WriteLine($"Much results: {(await MeasureSearch(service, "ana")).TotalSeconds} seconds.");
        Trace.WriteLine($"One result: {(await MeasureSearch(service, "shemesh")).TotalSeconds} seconds.");
    }

    private static async Task<TimeSpan> MeasureSearch(IContentProvider service, string query)
    {
        Stopwatch sw = new();
        sw.Start();

        _ = await service.SearchAsync(query);

        sw.Stop();
        return sw.Elapsed;
    }

    [TestMethod]
    public async Task SdarotTVSeasonsTest()
    {
        HoradotService service = new();
        await service.InitializeAsync(false);

        var show = new SdarotTVShowInformation
        {
            Id = 1,
            Name = "Family Guy",
            NameHe = "איש משפחה",
            ImageName = "1.jpg",
            Year = "1999"
        };

        Stopwatch sw = new();
        sw.Start();

        _ = await service.GetSeasonsAsync(show);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }

    [TestMethod]
    public async Task SdarotTVEpisodesTest()
    {
        HoradotService service = new();
        await service.InitializeAsync(false);

        SeasonInformation season = new()
        {
            Id = 4,
            Index = 3,
            Name = "4",
            ProviderName = "SdarotTV",
            Show = new SdarotTVShowInformation
            {
                Id = 1,
                Name = "Family Guy",
                NameHe = "איש משפחה",
                ImageName = "1.jpg",
                Year = "1999"
            }
        };

        Stopwatch sw = new();
        sw.Start();

        _ = await service.GetEpisodesAsync(season);

        sw.Stop();

        Trace.WriteLine($"Seasons retrieving: {sw.Elapsed.TotalSeconds} seconds.");
    }
}
