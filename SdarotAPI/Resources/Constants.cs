namespace SdarotAPI.Resources;

internal static class Constants
{
    public static class SdarotUrls
    {
        public const string SdarotUrlSource = "https://raw.githubusercontent.com/yairp03/HoradotTV/master/Resources/sdarot-url.txt";

        public static string BaseDomain { get; set; } = "";
        public static string HomeUrl => $"https://www.{BaseDomain}/";
        public static string SearchUrl => $"{HomeUrl}search?term=";
        public static string WatchUrl => $"{HomeUrl}watch/";
        public static string ImageUrl => $"https://static.{BaseDomain}/series/";
        public static string TestUrl => $"{WatchUrl}1";
    }

    public static class XPathSelectors
    {
        // Episode/poster/container/title/content
        public const string SeriesPageSeriesName = "//*[@id=\"watchEpisode\"]/div[1]/div/h1/strong";
        // Episode/content/container/information/picturebox/image
        public const string SeriesPageSeriesImage = "//*[@id=\"watchEpisode\"]/div[2]/div/div[1]/div[1]/img";
        // Results/content/row/series
        public const string SearchPageResult = "//*[@id=\"seriesList\"]/div[2]/div/div";
        // content/description/nameHe
        public const string SearchPageResultInnerSeriesNameHe = "div/div/h4";
        // content/description/nameEn
        public const string SearchPageResultInnerSeriesNameEn = "div/div/h5";
        // Seasons/season
        public const string SeriesPageSeason = "//*[@id=\"season\"]/li";
        // Episodes/episode
        public const string SeriesPageEpisode = "//*[@id=\"episode\"]/li";
        // WaitTime/container
        public const string SeriesPageEpisodeWaitTime = "//*[@id=\"waitTime\"]/span";
        // Loading/container/error/errorMessage
        public const string SeriesPageErrorMessage = "//*[@id=\"loading\"]/div/div[2]/h3";
    }

    public static class IdSelectors
    {
        public const string ProceedButtonId = "proceed";
        public const string EpisodeMedia = "videojs_html5_api";
    }

    public const int WaitTime = 30;
    public const string Error2Message = "שגיאה 2!";
}
