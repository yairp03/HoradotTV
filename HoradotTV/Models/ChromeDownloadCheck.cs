using HoradotTV.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace HoradotTV.Models
{
    internal class ChromeDownloadCheck : IDependencyCheck
    {
        string IDependencyCheck.LoadingText => "מוודא התקנת כרום";

        public string FixProblemUrl => "https://ksp.co.il/web/item/8272";

        public async Task<bool> RunCheckAsync()
        {
            try
            {
                await Task.Delay(2000);
                await new ChromeDriverInstaller().GetChromeVersion();
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
}
