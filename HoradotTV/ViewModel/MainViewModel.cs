using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SdarotAPI;
using SdarotAPI.Model;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HoradotTV.ViewModel;

internal partial class MainViewModel : BaseViewModel
{
    readonly SdarotDriver driver = new();

    [ObservableProperty]
    string searchQuery = "";

    [ObservableProperty]
    string errorText = "";

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(SearchPicker))]
    SeriesInformation[]? searchResults;

    public bool SearchPicker => SearchResults != null;

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(SeriesSelected))]
    [AlsoNotifyChangeFor(nameof(SeriesSeasons))]
    SeriesInformation? selectedSeries;

    public bool SeriesSelected => SelectedSeries != null;

    public ObservableCollection<bool> ModeArray { get; set; } = new ObservableCollection<bool>() { true, false, false };

    public int SelectedMode => ModeArray.IndexOf(true);

    public bool IsNotSeriesMode => SelectedMode != 2;

    public bool IsEpisodesMode => SelectedMode == 0;

    public SeasonInformation[]? SeriesSeasons { get; set; }

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CanDownload))]
    SeasonInformation? selectedSeason;

    public EpisodeInformation[]? SeasonEpisodes { get; set; }

    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CanDownload))]
    EpisodeInformation? selectedEpisode;

    [ObservableProperty]
    int episodesAmount = 1;

    [ObservableProperty]
    string downloadLocation = Properties.Settings.Default.DownloadLocation;

    public bool CanDownload => SelectedMode == 2 || (SelectedSeason is not null && (SelectedMode == 1 || (SelectedEpisode is not null && SelectedMode == 0)));

    public MainViewModel()
    {
        if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DownloadLocation))
        {
            ChangeDownloadLocation(KnownFolders.Downloads.Path);
        }
        ModeArray.CollectionChanged += ModeArray_CollectionChanged;
    }

    private void ModeArray_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        ModeChanged();
    }

    private void ModeChanged()
    {
        IsBusy = true;
        if (SelectedMode == 0)
        {
            EpisodesAmount = 1;
        }
        else if (SelectedMode == 1 && SeasonEpisodes is not null)
        {
            EpisodesAmount = SeasonEpisodes.Length;
        }
        OnPropertyChanged(nameof(CanDownload));
        OnPropertyChanged(nameof(IsNotSeriesMode));
        OnPropertyChanged(nameof(IsEpisodesMode));
        IsBusy = false;
    }

    public async Task Initialize()
    {
        IsBusy = true;
        await driver.Initialize();
        IsBusy = false;
    }

    public void Quit()
    {
        driver.Shutdown();
    }

    [ICommand]
    public async Task SearchSeries()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ErrorText = "";
        SearchResults = null;
        SelectedSeries = null;

        var results = await driver.SearchSeries(SearchQuery);

        if (results.Length == 0)
        {
            ErrorText = Properties.Resources.SeriesNotFound;
        }
        else if (results.Length == 1)
        {
            await SelectSeries(results[0]);
        }
        else
        {
            SearchResults = results;
        }
        ModeArray[0] = true;
        ModeArray[1] = false;
        ModeArray[2] = false;
        IsBusy = false;
    }

    [ICommand]
    private async Task SelectSeries(SeriesInformation series)
    {
        IsBusy = true;
        SearchResults = null;
        SeasonEpisodes = null;
        OnPropertyChanged(nameof(SeasonEpisodes));
        SeriesSeasons = await driver.GetSeasonsAsync(series);
        SelectedSeries = series;
        IsBusy = false;
    }

    public async Task SeasonChanged()
    {
        IsBusy = true;
        if (SelectedSeason is not null)
        {
            SeasonEpisodes = await driver.GetEpisodesAsync(SelectedSeason!);
            if (SelectedMode == 1)
            {
                EpisodesAmount = SeasonEpisodes.Length;
            }
            OnPropertyChanged(nameof(SeasonEpisodes));
        }
        IsBusy = false;
    }

    public void ChangeDownloadLocation(string newLocation)
    {
        Properties.Settings.Default.DownloadLocation = newLocation;
        DownloadLocation = newLocation;
    }
}
