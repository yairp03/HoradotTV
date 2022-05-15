using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HoradotTV.Models
{
    internal class SdarotConnectionCheck : IDependencyCheck
    {
        public string LoadingText => "בודק חיבור לאתר";

        public string FixProblemUrl => "https://ksp.co.il/web/item/159960";

        public async Task<bool> RunCheckAsync()
        {
            await Task.Delay(2000);
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://sdarot.tv/");
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
