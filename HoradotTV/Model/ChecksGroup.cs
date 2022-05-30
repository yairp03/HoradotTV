using CommunityToolkit.Mvvm.ComponentModel;
using HoradotTV.Services.Checks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HoradotTV.Model;

[ObservableObject]
internal partial class ChecksGroup
{
    public string? FixProblemUrl { get; set; }

    [ObservableProperty]
    private string? imageUri;

    [ObservableProperty]
    private string? displayText;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isDone;

    [ObservableProperty]
    private bool isFailed;

    public List<IDependencyCheck> Checks { get; set; }

    public ChecksGroup(string imageUri, List<IDependencyCheck> checks)
    {
        ImageUri = imageUri;
        Checks = checks;
    }

    public async Task<bool> RunChecksAsync()
    {
        IsBusy = true;
        foreach (var check in Checks)
        {
            DisplayText = check.LoadingText;
            if (!await check.RunCheckAsync())
            {
                IsBusy = false;
                FixProblemUrl = check.FixProblemUrl;
                IsFailed = true;
                return false;
            }
        }
        DisplayText = "";
        IsBusy = false;
        IsDone = true;
        return true;
    }

    public void Reset()
    {
        IsDone = false;
        IsFailed = false;
        IsBusy = false;
    }
}
