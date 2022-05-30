using HoradotTV.Resources;
using System.Net.Http;
using System.Threading.Tasks;

namespace HoradotTV.Services.Checks;

internal class SdarotConnectionCheck : IDependencyCheck
{
    public string LoadingText => "בודק חיבור לאתר";

    public string FixProblemUrl => "https://github.com/yairp03/HoradotTV/wiki/SdarotTV-connection-problem";

    public async Task<bool> RunCheckAsync()
    {
        await Task.Delay(500);
        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync(Constants.SdarotTV);
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
