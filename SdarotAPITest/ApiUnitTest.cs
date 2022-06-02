using SdarotAPI;

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
            await driver.Initialize(false);
            var series = (await driver.SearchSeries("שמש"))[0];
            var seasons = await driver.GetSeasonsAsync(series);

            var season2 = await driver.GetEpisodesAsync(seasons[1]);
            Assert.AreEqual(season2.Length, 22);

            var episodes = await driver.GetEpisodesAsync(season2[10], 150);
            Assert.AreNotEqual(episodes.Length, 150);

            var seriesEpisodes = await driver.GetEpisodesAsync(series);
            Assert.AreEqual(seriesEpisodes.Length, 142);

            var episode = await driver.GetEpisodeMediaDetailsAsync(episodes[0]) ?? throw new ArgumentNullException();
            await SdarotHelper.DownloadEpisode(episode, @"C:\Users\yairp\Desktop\episode.mp4");
        }
        finally
        {
            driver.Shutdown();
        }
    }
}