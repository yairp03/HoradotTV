namespace HoradotAPI.Interfaces;

public abstract class BaseContentProvider : IContentProvider
{
    public abstract string Name { get; }
    public Task<(bool success, string errorMessage)> InitializeAsync() => InitializeAsync(true);
    public abstract Task<(bool success, string errorMessage)> InitializeAsync(bool doChecks);
    public abstract Task<IEnumerable<MediaInformation>> SearchAsync(string query);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media) =>
        PrepareDownloadAsync(media, null);

    public Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media, IProgress<double>? progress) =>
        PrepareDownloadAsync(media, progress, default(CancellationToken));

    public abstract Task<MediaDownloadInformation?> PrepareDownloadAsync(MediaInformation media,
        IProgress<double>? progress, CancellationToken ct);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream) =>
        DownloadAsync(media, resolution, stream, null);

    public Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress) => DownloadAsync(media, resolution, stream, progress, default(CancellationToken));

    public abstract Task DownloadAsync(MediaDownloadInformation media, int resolution, Stream stream,
        IProgress<long>? progress, CancellationToken ct);
}
