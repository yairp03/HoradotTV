using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HoradotTV.Model;
using HoradotTV.Services.Checks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HoradotTV.ViewModel;

internal partial class StartupChecksViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool isDone;

    [ObservableProperty]
    private bool autoProceed;

    public List<ChecksGroup> ChecksGroups { get; set; }
    public string? FixProblemUrl { get; set; }
    public Action? ProceedAction { get; internal set; }

    public StartupChecksViewModel()
    {
        AutoProceed = Properties.Settings.Default.AutoCloseStartupPage;
        ChecksGroups = new List<ChecksGroup>()
        {
            new ChecksGroup("/Resources/Images/chrome-logo.png", new()
            {
                new ChromeDownloadCheck(),
                new ChromeDriverCheck()
            }),
            new ChecksGroup("/Resources/Images/sdarot-logo.png", new()
            {
                new SdarotConnectionCheck()
            }),
        };
    }

    [ICommand]
    internal async Task RunChecksAsync()
    {
        IsBusy = true;
        bool success = true;
        Reset();
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
            Proceed();
        }
    }

    [ICommand]
    private void Proceed()
    {
        ProceedAction?.Invoke();
    }

    private void Reset()
    {
        IsDone = false;
        foreach (var group in ChecksGroups)
        {
            group.Reset();
        }
    }

    [ICommand]
    internal void FixProblem()
    {
        Process.Start(new ProcessStartInfo(FixProblemUrl!) { UseShellExecute = true });
    }

    [ICommand]
    internal void CheckBoxChanged()
    {
        Properties.Settings.Default.AutoCloseStartupPage = AutoProceed;
    }
}
