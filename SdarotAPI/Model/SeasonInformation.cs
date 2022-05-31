namespace SdarotAPI.Model;

public class SeasonInformation
{
    public int SeasonNumber { get; set; }
    public string SeasonName { get; set; }
    public SeriesInformation Series { get; set; }

    public string SeasonUrl => $"{Series.SeriesUrl}/season/{SeasonNumber}";

    public SeasonInformation(int seasonNumber, string seasonName, SeriesInformation series)
    {
        SeasonNumber = seasonNumber;
        SeasonName = seasonName;
        Series = series;
    }
}
