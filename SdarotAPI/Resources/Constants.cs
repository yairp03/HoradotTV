using static System.Net.WebRequestMethods;

namespace SdarotAPI.Resources;

internal static class Constants
{
    public static class SdarotUrls
    {
        public const string SdarotUrlSource = "https://raw.githubusercontent.com/yairp03/HoradotTV/master/Resources/sdarot-url.txt";

        public static string BaseDomain { get; set; } = "";
        public static string HomeUrl => $"https://www.{BaseDomain}/";
        public static string LoginUrl => $"{HomeUrl}login";
        public static string SearchUrl => $"{HomeUrl}search?term=";
        public static string WatchUrl => $"{HomeUrl}watch/";
        public static string ImageUrl => $"https://static.{BaseDomain}/series/";
        public static string TestUrl => $"{WatchUrl}1";
        public static string AjaxWatchUrl => $"{HomeUrl}ajax/watch";
        public static string AllShowsUrl => $"{HomeUrl}ajax/index?srl=1";
    }

    public static class XPathSelectors
    {
        public const string MainPageLoginPanelButton = "//*[@id=\"slideText\"]/p/button";

        public const string SearchPageResult = "//*[@id=\"seriesList\"]/div[2]/div/div";
        public const string SearchPageResultInnerSeriesNameHe = "div/div/h4";
        public const string SearchPageResultInnerSeriesNameEn = "div/div/h5";

        public const string SeriesPageSeriesName = "//*[@id=\"watchEpisode\"]/div[1]/div/h1/strong";
        public const string SeriesPageSeriesImage = "//*[@id=\"watchEpisode\"]/div[2]/div/div[1]/div[1]/img";
        public const string SeriesPageSeason = "//*[@id=\"season\"]/li";

        public const string AjaxEpisode = "/li";
    }

    public const string LoginMessage = "התחברות לאתר";
    public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
}
