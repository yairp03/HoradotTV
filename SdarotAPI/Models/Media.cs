namespace SdarotAPI.Models;

public class Media
{
    [JsonPropertyName("VID")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("watch")] public Dictionary<int, string> ResolutionsLinks { get; init; } = new();

    [JsonIgnore]
    public string MaxResolutionLink => ResolutionsLinks[ResolutionsLinks.Max(resolution => resolution.Key)];
}
