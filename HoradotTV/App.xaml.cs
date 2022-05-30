using HoradotTV.Properties;
using System.Windows;

namespace HoradotTV;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Settings.Default.Save();
    }
}
