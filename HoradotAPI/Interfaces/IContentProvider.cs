namespace HoradotAPI.Interfaces;

public interface IContentProvider
{
    public string Name { get; }

    public Task<(bool success, string errorMessage)> InitializeAsync();
    public Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks);

    public Task<IEnumerable<MediaInformation>> SearchAsync(string query);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress,
        CancellationToken ct);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream, IProgress<long>? progress);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream, IProgress<long>? progress,
        CancellationToken ct);
}
