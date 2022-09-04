namespace SdarotAPI.Models;

public class SeriesInformation
{
    public string SeriesNameHe { get; set; }
    public string SeriesNameEn { get; set; }
    public int SeriesCode { get; set; }
    public string ImageUrl { get; set; }

    public string SeriesUrl => $"{Constants.SdarotUrls.WatchUrl}{SeriesCode}";

    public SeriesInformation(string seriesNameHe, string seriesNameEn, string imageUrl)
    {
        SeriesNameHe = seriesNameHe;
        SeriesNameEn = seriesNameEn;
        ImageUrl = imageUrl;
        SeriesCode = GetSeriesCodeFromImageUrl(ImageUrl);
    }

    public SeriesInformation(string seriesFullName, string imageUrl)
    {
        var names = seriesFullName.Split('/');
        SeriesNameHe = names[0].Trim();
        SeriesNameEn = names[1].Trim();
        ImageUrl = imageUrl;
        SeriesCode = GetSeriesCodeFromImageUrl(ImageUrl);
    }

    public static int GetSeriesCodeFromImageUrl(string imageUrl) => int.Parse(Regex.Match(imageUrl, "\\d+").Value);
}
