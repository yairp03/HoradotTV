namespace HoradotTV.Services.Checks;

internal class SdarotConnectionCheck : ICheck
{
    public string LoadingText => AppResource.CheckingConnectionToSite;

    public string FixProblemUrl => Constants.SdarotConnectionFixUrl;

    public async Task<bool> RunCheckAsync()
    {
        await Task.Delay(500);
        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync(await SdarotHelper.GetSdarotTestUrl());
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
