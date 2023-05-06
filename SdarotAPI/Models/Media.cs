namespace SdarotAPI.Models;

public class Media
{
    [JsonPropertyName("VID")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("Watch")] public Dictionary<int, string> ResolutionsLinks { get; } = new();
    [JsonIgnore] public string MaxResolutionLink => ResolutionsLinks[ResolutionsLinks.Max(resolution => resolution.Key)];
}
