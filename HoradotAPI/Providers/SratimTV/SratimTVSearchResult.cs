namespace HoradotAPI.Providers.SratimTV;

public record SratimTVSearchResult
{
    [JsonPropertyName("success")] public bool Success { get; init; }

    [JsonPropertyName("results")] public List<SratimTVMovieInformation> Results { get; init; } = new();
}
