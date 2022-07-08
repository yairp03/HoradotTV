namespace HoradotTV.Services.Checks;

internal class ChromeDownloadCheck : ICheck
{
    public string LoadingText => AppResource.EnsuringChromeDownload;

    public string FixProblemUrl => Constants.ChromeDownloadFixUrl;

    public async Task<bool> RunCheckAsync()
    {
        try
        {
            await ChromeDriverHelper.GetChromeVersion();
        }
        catch (Exception)
        {
            // Chrome is not installed
            return false;
        }

        return true;
    }
}
