namespace HoradotTV.ViewModels;

public partial class StartupViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool isDone;

    [ObservableProperty]
    private bool autoProceed;

    public List<ChecksGroup> ChecksGroups { get; set; }

    public string FixProblemUrl { get; set; }

    readonly IPreferences preferences;
    readonly IBrowser browser;

    public StartupViewModel(IPreferences preferences, IBrowser browser)
    {
        this.preferences = preferences;
        this.browser = browser;

        AutoProceed = this.preferences.Get(Constants.AutoProceedKey, false);

        ChecksGroups = new()
        {
            new(Constants.ChromeLogoUri, new()
            {
                new ChromeDownloadCheck(),
                new ChromeDriverCheck()
            }),
            new(Constants.SdarotLogoUri, new()
            {
                new SdarotConnectionCheck()
            })
        };
    }

    [RelayCommand]
    private async Task RunChecksAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;

        Reset();

        var success = true;
        foreach (var group in ChecksGroups)
        {
            if (!await group.RunChecksAsync())
            {
                FixProblemUrl = group.FixProblemUrl;
                success = false;
                break;
            }
        }

        IsDone = success;
        IsBusy = false;

        if (success && AutoProceed)
        {
            await ProceedAsync();
        }
    }

    private void Reset()
    {
        IsDone = false;
        foreach (var group in ChecksGroups)
        {
            group.Reset();
        }
    }

    [RelayCommand]
    private async Task FixProblemAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;

        await browser.OpenAsync(FixProblemUrl);

        IsBusy = false;
    }

    [RelayCommand]
    private async Task ProceedAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;

        await Shell.Current.GoToAsync($"{nameof(MainPage)}");

        IsBusy = false;
    }

    partial void OnAutoProceedChanged(bool value)
    {
        preferences.Set(Constants.AutoProceedKey, value);
    }
}
