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
            var res = await driver.SearchSeries("שמש");
            var res2 = await driver.GetSeasonsAsync(res[0]);
            var res3 = await driver.GetEpisodesAsync(res2[4]);
            Assert.AreEqual(res3.Length, 25);
        }
        finally
        {
            driver.Shutdown();
        }
    }
}