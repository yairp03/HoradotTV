namespace SdarotAPI.Model;

public class EpisodeInformation
{
    public int EpisodeNumber { get; set; }
    public int EpisodeIndex { get; set; }
    public string EpisodeName { get; set; }
    public SeasonInformation Season { get; set; }

    public string EpisodeUrl => $"{Season.SeasonUrl}/episode/{EpisodeNumber}";

    public EpisodeInformation(int episodeNumber, int episodeIndex, string episodeName, SeasonInformation season)
    {
        EpisodeNumber = episodeNumber;
        EpisodeIndex = episodeIndex;
        EpisodeName = episodeName;
        Season = season;
    }
}
