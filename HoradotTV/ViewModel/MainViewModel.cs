using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SdarotAPI;
using System;
using System.Threading.Tasks;

namespace HoradotTV.ViewModel;

internal partial class MainViewModel : BaseViewModel
{
    SdarotDriver driver = new();

    [ObservableProperty]
    string searchQuery = "";

    [ObservableProperty]
    bool searchPicker = false;

    [ObservableProperty]
    bool seriesSelected = false;

    [ICommand]
    public async Task SearchSeries()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        if (!driver.IsInitialized)
        {
            await driver.Initialize();
        }

        throw new NotImplementedException();

        IsBusy = false;
    }
}
