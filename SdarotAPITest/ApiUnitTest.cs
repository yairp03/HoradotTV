using SdarotAPI;
using SdarotAPI.Model;

namespace SdarotAPITest
{
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
                Assert.AreEqual(res.Length, 15);
            }
            finally
            {
                driver.Shutdown();
            }
        }
    }
}