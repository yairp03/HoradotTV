﻿namespace HoradotTV.Services.Checks;

internal class ChromeDriverCheck : ICheck
{
    public string LoadingText => AppResource.DownloadingChromeDriver;

    public string FixProblemUrl => Constants.ChromeDriverFixUrl;

    public async Task<bool> RunCheckAsync()
    {
        try
        {
            await ChromeDriverHelper.Install();
        }
        catch (Exception)
        {
            // Error while downloading chrome driver
            return false;
        }

        return true;
    }
}
