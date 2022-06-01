using HoradotTV.ViewModel;
using System.Windows;

namespace HoradotTV.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainViewModel? vm = DataContext as MainViewModel;
        await vm!.Initialize();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        MainViewModel? vm = DataContext as MainViewModel;
        vm!.Quit();
    }

    private async void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        MainViewModel? vm = DataContext as MainViewModel;
        await vm!.SeasonChanged();
    }
}
