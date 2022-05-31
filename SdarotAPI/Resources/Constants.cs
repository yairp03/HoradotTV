namespace SdarotAPI.Resources;

internal static class Constants
{
    public static class SdarotUrls
    {
        public const string BaseDomain = "sdarot.to";
        public const string HomeUrl = $"https://{BaseDomain}/";
        public const string SearchUrl = $"{HomeUrl}search?term=";
        public const string WatchUrl = $"{HomeUrl}watch/";
        public const string ImageUrl = $"https://static.{BaseDomain}/series/";
        public const string TestUrl = $"{WatchUrl}1";
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
    }
}
