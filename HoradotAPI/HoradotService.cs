namespace HoradotAPI;

public class HoradotService : IShowProvider
{
    private IContentProvider[] contentProviders = Array.Empty<IContentProvider>();
    private bool isInitialized;

    public string Name => "Horadot";

    public Task<(bool success, string errorMessage)> InitializeAsync() => InitializeAsync(true);

    public async Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks)
    {
        if (isInitialized)
        {
            throw new ServiceAlreadyInitialized();
        }

        List<string> errorMessages = new();
        List<IContentProvider> providersList = new();

        // Add SdarotTVService
        SdarotTVService sdarotTVService = new();
        (bool success, string message) currResult = await sdarotTVService.InitializeAsync();
        if (currResult.success)
        {
            providersList.Add(sdarotTVService);
        }
        else
        {
            errorMessages.Add(currResult.message);
        }

        // Add HoradotTVService

        contentProviders = providersList.ToArray();
        isInitialized = true;

        string errorMessage = string.Join("\n", errorMessages);
        return (string.IsNullOrWhiteSpace(errorMessage), errorMessage);
    }

    public async Task<IEnumerable<MediaInformation>> SearchAsync(string query)
    {
        if (!isInitialized)
        {
            throw new ServiceNotInitialized();
        }

        List<MediaInformation> results = new();
        foreach (var provider in contentProviders)
        {
            results.AddRange(await provider.SearchAsync(query));
        }

        return results;
    }

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media) =>
        PrepareDownloadAsync(media, null);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress) =>
        PrepareDownloadAsync(media, progress, default(CancellationToken));

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress,
        CancellationToken ct)
    {
        var provider = GetProvider(media.ProviderName);
        if (provider is null)
        {
            throw new ProviderNotFoundException(media.ProviderName);
        }

        return provider.PrepareDownloadAsync(media, progress, ct);
    }

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream) =>
        DownloadAsync(media, resolution, stream, null);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress) => DownloadAsync(media, resolution, stream, progress, default(CancellationToken));

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream, IProgress<long>? progress,
        CancellationToken ct)
    {
        var provider = GetProvider(media.Information.ProviderName);
        if (provider is null)
        {
            throw new ProviderNotFoundException(media.Information.ProviderName);
        }

        return provider.DownloadAsync(media, resolution, stream, progress, ct);
    }

    public Task<IEnumerable<SeasonInformation>> GetSeasonsAsync(ShowInformation show)
    {
        if (GetProvider(show.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(show.ProviderName);
        }

        return provider.GetSeasonsAsync(show);
    }

    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(SeasonInformation season)
    {
        if (GetProvider(season.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(season.ProviderName);
        }

        return provider.GetEpisodesAsync(season);
    }

    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(EpisodeInformation firstEpisode, int maxEpisodeAmount)
    {
        if (GetProvider(firstEpisode.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(firstEpisode.ProviderName);
        }

        return provider.GetEpisodesAsync(firstEpisode, maxEpisodeAmount);
    }

    public Task<IEnumerable<EpisodeInformation>> GetEpisodesAsync(ShowInformation show)
    {
        if (GetProvider(show.ProviderName) is not IShowProvider provider)
        {
            throw new ProviderNotFoundException(show.ProviderName);
        }

        return provider.GetEpisodesAsync(show);
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
