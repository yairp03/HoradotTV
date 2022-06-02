using HoradotTV.Resources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace HoradotTV.Services.Checks;

internal class ChromeDriverCheck : IDependencyCheck
{
    public string LoadingText => "מוריד דרייבר לכרום";

    public string FixProblemUrl => "https://github.com/yairp03/HoradotTV/wiki/Chrome-driver-problem";

    public async Task<bool> RunCheckAsync()
    {
        try
        {
            if (Properties.Settings.Default.IsCheckDelay)
            {
                await Task.Delay(800);
            }
            await ChromeDriverInstaller.Install();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception when installing chrome driver");
            return false;
        }
        return true;
    }
}
