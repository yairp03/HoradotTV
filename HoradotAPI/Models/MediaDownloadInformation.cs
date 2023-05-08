namespace HoradotAPI.Models;

public record MediaDownloadInformation
{
    public required MediaInformation Information { get; init; }
    public required ImmutableDictionary<int, string> Resolutions { get; init; }
}
