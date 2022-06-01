using HoradotTV.Resources;
using SdarotAPI;
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
        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync(await GetSdarotTestUrl());
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public static async Task<string> GetSdarotTestUrl()
    {
        return $"https://{await SdarotHelper.RetrieveSdarotDomain()}/watch/1";
    }
}
