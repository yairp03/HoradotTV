using System.Net;

namespace SdarotAPI.Model;

public class EpisodeMediaDetails
{
    public string MediaUrl { get; set; }
    public CookieContainer Cookies { get; set; }

    public EpisodeMediaDetails(string mediaUrl, CookieContainer cookies)
    {
        MediaUrl = mediaUrl;
        Cookies = cookies;
    }
}
