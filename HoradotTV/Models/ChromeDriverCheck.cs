using HoradotTV.Services;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace HoradotTV.Models
{
    internal class ChromeDriverCheck : IDependencyCheck
    {
        public string LoadingText => "מוריד דרייבר לכרום";

        public string FixProblemUrl => "https://ksp.co.il/web/item/130391";

        public async Task<bool> RunCheckAsync()
        {
            var chromeDriverInstaller = new ChromeDriverInstaller();
            try
            {
                await Task.Delay(2000);
                await chromeDriverInstaller.Install();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception when installing chrome driver");
                return false;
            }
            return true;
        }
    }
}
