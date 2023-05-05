namespace SdarotAPI.Models;

public class EpisodeInformation
{
    public string Name { get; set; }
    public int Number { get; set; }
    public int Index { get; set; }
    public SeasonInformation Season { get; set; }

    [JsonIgnore]
    public ShowInformation Show => Season.Show;

    [JsonIgnore]
    public string Url => $"{Season.Url}/episode/{Number}";

    public EpisodeInformation(int episodeNumber, int episodeIndex, string episodeName, SeasonInformation season)
    {
        Number = episodeNumber;
        Index = episodeIndex;
        Name = episodeName;
        Season = season;
    }

    public override string ToString() => $"Episode S{Season.Number:D2}E{Number:D2}";
}
