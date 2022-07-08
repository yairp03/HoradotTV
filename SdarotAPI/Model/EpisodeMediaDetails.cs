namespace SdarotAPI.Model;

public class EpisodeMediaDetails
{
    public string MediaUrl { get; set; }
    public CookieContainer Cookies { get; set; }
    public EpisodeInformation Information { get; set; }

    public EpisodeMediaDetails(string mediaUrl, CookieContainer cookies, EpisodeInformation information)
    {
        MediaUrl = mediaUrl;
        Cookies = cookies;
        Information = information;
    }
}
