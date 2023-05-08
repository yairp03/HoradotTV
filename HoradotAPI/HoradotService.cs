namespace HoradotAPI;

public class HoradotService : BaseShowProvider, IShowProvider
{
    private IContentProvider[] contentProviders = Array.Empty<IContentProvider>();
    private bool isInitialized;

    public override string Name => "Horadot";

    public override async Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks)
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }

        List<string> errorMessages = new();
        List<IContentProvider> providersList = new()
        {
            new SdarotTVService(),
            new SratimTVService()
        };

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

        string errorMessage = string.Join("\n", errorMessages);
        return (string.IsNullOrWhiteSpace(errorMessage), errorMessage);
    }

    public void InitializeAsync(IEnumerable<IContentProvider> providers)
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }

        contentProviders = providers.ToArray();
        isInitialized = true;
    }

    public override async Task<IEnumerable<MediaInformation>> SearchAsync(string query)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        var searchTasks = contentProviders.Select(provider => provider.SearchAsync(query)).ToList();
        await Task.WhenAll(searchTasks);

        List<MediaInformation> results = new();
        foreach (var searchTask in searchTasks)
        {
            results.AddRange(await searchTask);
        }

        return results;
    }

    public override Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media,
        IProgress<double>? progress, CancellationToken ct)
    {
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
        var provider = GetProvider(media.Information.ProviderName);
        if (provider is null)
        {
            throw new ProviderNotFoundException(media.Information.ProviderName);
        }

        return provider.DownloadAsync(media, resolution, stream, progress, ct);
    }

    public override Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        if (GetProvider(show.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(show.ProviderName);
        }

        return provider.GetSeasonsAsync(show);
    }

    public override Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        if (GetProvider(season.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(season.ProviderName);
        }

        return provider.GetEpisodesAsync(season);
    }

    public new Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode,
        int maxEpisodeAmount)
    {
        if (GetProvider(firstEpisode.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(firstEpisode.ProviderName);
        }

        return provider.GetEpisodesAsync(firstEpisode, maxEpisodeAmount);
    }

    public new Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show)
    {
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

    public Task<bool> Login(string providerName, string username, string password) =>
        GetProvider(providerName) is not IAuthContentProvider provider
            ? Task.FromResult(true)
            : provider.Login(username, password);
}
