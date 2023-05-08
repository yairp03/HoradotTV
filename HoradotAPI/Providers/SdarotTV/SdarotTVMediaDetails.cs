namespace HoradotAPI.Providers.SdarotTV;

public class SdarotTVMediaDetails
{
    [JsonPropertyName("VID")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("watch")] public Dictionary<int, string> Resolutions { get; init; } = new();
}
