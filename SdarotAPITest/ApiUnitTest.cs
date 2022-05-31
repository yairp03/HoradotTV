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
            driver.Initialize(false);
            var res = await driver.SearchSeries("שמש");
            var res2 = await driver.GetSeasonsAsync(res[0]);
            Assert.AreEqual(res2.Length, 6);
        }
        finally
        {
            driver.Shutdown();
        }
    }
}