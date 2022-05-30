using SdarotAPI;
using SdarotAPI.Model;

namespace SdarotAPITest
{
    [TestClass]
    public class ApiUnitTest
    {
        [TestMethod]
        public void DriverTest()
        {
            SdarotDriver driver = new();
            try
            {
                driver.Initialize(false);
                var res = driver.SearchSeries("בין השמשות")[0];
                Assert.AreEqual(
                    res.SeriesNameHe,
                    "בין השמשות"
                );
                Assert.AreEqual(
                    res.ImageUrl,
                    "https://static.sdarot.to/series/3239.jpg"
                );
            }
            finally
            {
                driver.Shutdown();
            }
        }

        [TestMethod]
        public void RegexTest()
        {
            Assert.AreEqual(3239, SeriesInformation.GetSeriesCodeFromImageUrl("https://static.sdarot.to/series/3239.jpg"));
        }
    }
}