using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SdarotAPI;
using SdarotAPI.Model;
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
    SeasonInformation? selectedSeason;

    public EpisodeInformation[]? SeasonEpisodes { get; set; }

    [ObservableProperty]
    EpisodeInformation? selectedEpisode;

    public MainViewModel()
    {
        ModeArray.CollectionChanged += ModeArray_CollectionChanged;
    }

    private void ModeArray_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsNotSeriesMode));
        OnPropertyChanged(nameof(IsEpisodesMode));
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
            SelectSeries(results[0]);
        }
        else
        {
            SearchResults = results;
        }
        IsBusy = false;
    }

    [ICommand]
    private async void SelectSeries(SeriesInformation series)
    {
        IsBusy = true;
        SearchResults = null;
        var seasons = await driver.GetSeasonsAsync(series);
        SeriesSeasons = seasons;
        SelectedSeries = series;
        IsBusy = false;
    }

    public async Task SeasonChanged()
    {
        if (SelectedSeason is not null)
        {
            SeasonEpisodes = await driver.GetEpisodesAsync(SelectedSeason!);
            OnPropertyChanged(nameof(SeasonEpisodes));
        }
    }
}
