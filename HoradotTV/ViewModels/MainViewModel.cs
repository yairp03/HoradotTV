namespace HoradotTV.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly SdarotDriver driver = new();

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private string errorText;

    [ObservableProperty]
    private SeriesInformation[] searchResults;

    [ObservableProperty]
    private SeriesInformation selectedSeries;


}
