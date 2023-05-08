namespace HoradotAPI.Models;

public record SeasonInformation : MediaInformation
{
    public required int Index { get; init; }

    public required ShowInformation Show { get; init; }

    [JsonIgnore] public override string RelativeName => $"Season {Id:D2}";

    public override string ToString() => $"{Show} {RelativeName}";
}
