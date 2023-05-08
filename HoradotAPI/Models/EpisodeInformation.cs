namespace HoradotAPI.Models;

public record EpisodeInformation : MediaInformation
{
    public required int Index { get; init; }

    public required SeasonInformation Season { get; init; }
    [JsonIgnore] public ShowInformation Show => Season.Show;

    [JsonIgnore] public override string RelativeName => $"Episode S{Season.Id:D2}E{Id:D2}";

    public override string ToString() => $"{Season} {RelativeName}";
}
