namespace SdarotAPI.Models;

public class SeasonInformation
{
    public string Name { get; set; }
    public int Number { get; set; }
    public int Index { get; set; }
    public ShowInformation Show { get; set; }

    [JsonIgnore]
    public string Url => $"{Show.Url}/season/{Number}";

    public SeasonInformation(int seasonNumber, int seasonIndex, string seasonName, ShowInformation show)
    {
        Number = seasonNumber;
        Index = seasonIndex;
        Name = seasonName;
        Show = show;
    }

    public override string ToString() => $"Season {Number:D2}";
}
