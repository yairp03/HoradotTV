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
        }
        finally
        {
            driver.Shutdown();
        }
    }
}