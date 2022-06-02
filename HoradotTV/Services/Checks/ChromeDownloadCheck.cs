using HoradotTV.Resources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace HoradotTV.Services.Checks;

internal class ChromeDownloadCheck : IDependencyCheck
{
    string IDependencyCheck.LoadingText => "מוודא התקנת כרום";

    public string FixProblemUrl => "https://github.com/yairp03/HoradotTV/wiki/Chrome-download-problem";

    public async Task<bool> RunCheckAsync()
    {
        try
        {
            if (Properties.Settings.Default.IsCheckDelay)
            {
                await Task.Delay(800);
            }
            await ChromeDriverInstaller.GetChromeVersion();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error occured while looking up for chrome installation");
            //got an error, probably chrome is not installed(not sure however).
            return false;
        }
        return true;
    }
}