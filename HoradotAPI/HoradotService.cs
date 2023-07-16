namespace HoradotAPI;

public class HoradotService : BaseShowProvider, IShowProvider
{
    private IContentProvider[] contentProviders = Array.Empty<IContentProvider>();
    private bool isInitialized;

    public override string Name => "Horadot";

    private static List<IContentProvider> GetDefaultProvidersList() =>
        new()
        {
            new SdarotTVService(),
            new SratimTVService()
        };

    public override Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks) =>
        InitializeAsync(doChecks, GetDefaultProvidersList());

    private async Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks,
        List<IContentProvider> providersList)
    {
        AssertNotInitialized();

        List<string> errorMessages = new();

        var initializeTasks = providersList.Select(provider => provider.InitializeAsync(doChecks)).ToList();
        await Task.WhenAll(initializeTasks);
        for (int i = providersList.Count - 1; i >= 0; i--)
        {
            (bool success, string message) = await initializeTasks[i];
            if (success)
            {
                continue;
            }

            errorMessages.Add(message);
            providersList.RemoveAt(i);
        }

        contentProviders = providersList.ToArray();
        isInitialized = true;

        string errorMessage = string.Join(Environment.NewLine, errorMessages);
        return (string.IsNullOrWhiteSpace(errorMessage), errorMessage);
    }

    private async Task<IEnumerable<T>> GatherData<T>(Func<IContentProvider, Task<IEnumerable<T>>> selector)
    {
        AssertInitialized();

        var allTasks = contentProviders.Select(selector).ToList();
        await Task.WhenAll(allTasks);

        List<T> allData = new();
        foreach (var task in allTasks)
        {
            allData.AddRange(await task);
        }

        return allData;
    }

    public override Task<IEnumerable<MediaInformation>> SearchAsync(string query) =>
        GatherData(provider => provider.SearchAsync(query));

    public override Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media,
        IProgress<double>? progress, CancellationToken ct)
    {
        AssertInitialized();

        var provider = GetProvider(media.ProviderName);
        if (provider is null)
        {
            throw new ProviderNotFoundException(media.ProviderName);
        }

        return provider.PrepareDownloadAsync(media, progress, ct);
    }

    public override Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress, CancellationToken ct)
    {
        AssertInitialized();

        if (GetProvider(media.Information.ProviderName) is not { } provider)
        {
            throw new ProviderNotFoundException(media.Information.ProviderName);
        }

        return provider.DownloadAsync(media, resolution, stream, progress, ct);
    }

    public override Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        AssertInitialized();

        if (GetProvider(show.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(show.ProviderName);
        }

        return provider.GetSeasonsAsync(show);
    }

    public override Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        AssertInitialized();

        if (GetProvider(season.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(season.ProviderName);
        }

        return provider.GetEpisodesAsync(season);
    }

    public new Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode,
        int maxEpisodeAmount)
    {
        AssertInitialized();

        if (GetProvider(firstEpisode.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(firstEpisode.ProviderName);
        }

        return provider.GetEpisodesAsync(firstEpisode, maxEpisodeAmount);
    }

    public new Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show)
    {
        AssertInitialized();

        if (GetProvider(show.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(show.ProviderName);
        }

        return provider.GetEpisodesAsync(show);
    }

    private IContentProvider? GetProvider(string providerName) =>
        contentProviders.FirstOrDefault(p => p.Name == providerName);

    public Task<bool> IsLoggedIn(string providerName) => GetProvider(providerName) is not IAuthContentProvider provider
        ? Task.FromResult(true)
        : provider.IsLoggedIn();

    public async Task<bool> Login(string providerName, string username, string password)
    {
        AssertInitialized();

        var provider = GetProvider(providerName) as IAuthContentProvider;
        return provider == null || await provider.Login(username, password);
    }

    private void AssertInitialized()
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }
    }

    private void AssertNotInitialized()
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }
    }
}
