namespace HoradotAPI.Providers.SdarotTV;

public record SdarotTVMediaDetails
{
    [JsonPropertyName("VID")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("watch")] public Dictionary<int, string> Resolutions { get; init; } = new();
}
