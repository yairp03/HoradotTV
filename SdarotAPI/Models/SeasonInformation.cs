namespace SdarotAPI.Models;

public class SeasonInformation
{
    public string Name { get; }
    public int Number { get; }
    public int Index { get; }
    public ShowInformation Show { get; }

    public SeasonInformation(string name, int number, int index, ShowInformation show)
    {
        Name = name;
        Number = number;
        Index = index;
        Show = show;
    }

    public override string ToString() => $"Season {Number:D2}";
}
