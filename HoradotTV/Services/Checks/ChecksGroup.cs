namespace HoradotTV.Services.Checks;

[ObservableObject]
public partial class ChecksGroup
{
    public string FixProblemUrl { get; set; }

    [ObservableProperty]
    private string imageUri;

    [ObservableProperty]
    private string displayText;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isSuccess;

    [ObservableProperty]
    private bool isFail;

    public List<ICheck> Checks { get; set; }

    public ChecksGroup(string imageUri, List<ICheck> checks)
    {
        ImageUri = imageUri;
        Checks = checks;
    }

    public async Task<bool> RunChecksAsync()
    {
        Reset();
        IsBusy = true;
        foreach (var check in Checks)
        {
            DisplayText = check.LoadingText;
            if (!await check.RunCheckAsync())
            {
                FixProblemUrl = check.FixProblemUrl;
                IsFail = true;
                IsBusy = false;
                return false;
            }
        }
        DisplayText = "";
        IsSuccess = true;
        IsBusy = false;
        return true;
    }

    public void Reset()
    {
        IsSuccess = false;
        IsFail = false;
        IsBusy = false;
    }
}
