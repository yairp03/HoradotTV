namespace SdarotAPI.Model;

public class EpisodeInformation
{
    public int EpisodeNumber { get; set; }
    public string EpisodeName { get; set; }
    public SeasonInformation Season { get; set; }

    public string EpisodeUrl => $"{Season.SeasonUrl}/episode/{EpisodeNumber}";

    public EpisodeInformation(int episodeNumber, string episodeName, SeasonInformation season)
    {
        EpisodeNumber = episodeNumber;
        EpisodeName = episodeName;
        Season = season;
    }
}
