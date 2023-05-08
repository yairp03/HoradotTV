namespace HoradotAPI.Models;

[JsonDerivedType(typeof(MediaInformation), "base")]
[JsonDerivedType(typeof(MovieInformation), "movie")]
[JsonDerivedType(typeof(ShowInformation), "show")]
[JsonDerivedType(typeof(SeasonInformation), "season")]
[JsonDerivedType(typeof(EpisodeInformation), "episode")]
public record MediaInformation
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
    public string NameHe { get; init; } = string.Empty;

    [JsonIgnore] public virtual string RelativeName => Name;

    public string ProviderName { get; init; } = string.Empty;

    public override string ToString() => Name;
}
