using CommunityToolkit.Mvvm.ComponentModel;

namespace HoradotTV.ViewModel;

[ObservableObject]
internal partial class BaseViewModel
{
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(IsNotBusy))]
    bool isBusy;

    public bool IsNotBusy => !IsBusy;
}
