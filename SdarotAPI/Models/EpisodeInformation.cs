namespace SdarotAPI.Models;

public class EpisodeInformation
{
    public string Name { get; }
    public int Number { get; }
    public int Index { get; }
    public SeasonInformation Season { get; }

    [JsonIgnore] public ShowInformation Show => Season.Show;

    public EpisodeInformation(string name, int number, int index, SeasonInformation season)
    {
        Name = name;
        Number = number;
        Index = index;
        Season = season;
    }

    public override string ToString() => $"Episode S{Season.Number:D2}E{Number:D2}";
}
