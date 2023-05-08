namespace HoradotAPI.Providers.SratimTV;

public record SratimTVMediaDetails
{
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("watch")] public Dictionary<int, string> Resolutions { get; init; } = new();
}
