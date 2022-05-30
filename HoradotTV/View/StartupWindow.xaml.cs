using HoradotTV.ViewModel;
using System.Windows;

namespace HoradotTV.View
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {

        public StartupWindow()
        {
            InitializeComponent();

            Loaded += StartupWindow_Loaded;
        }

        private async void StartupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartupChecksViewModel? vm = DataContext as StartupChecksViewModel;
            vm!.ProceedAction = () => GoToMainWindow();
            await vm!.RunChecksAsync();
        }

        void GoToMainWindow()
        {
            new MainWindow().Show();
            Close();
        }
    }
}
