using System.ComponentModel;

namespace SdarotAPI.Models;

public partial class SeriesInformation
{
    [JsonPropertyName("heb")]
    public string SeriesNameHe { get; set; }
    [JsonPropertyName("eng")]
    public string SeriesNameEn { get; set; }
    [JsonPropertyName("id")]
    public string SeriesId { get; set; }

    public int SeriesCode
    {
        get
        {
            return int.Parse(SeriesId);
        }
        set
        {
            SeriesId = value.ToString();
        }
    }

    [JsonIgnore]
    public string ImageUrl => $"{Constants.SdarotUrls.ImageUrl}{SeriesCode}.jpg";

    [JsonIgnore]
    public string SeriesUrl => $"{Constants.SdarotUrls.WatchUrl}{SeriesCode}";

    public SeriesInformation()
    {
    }

    public SeriesInformation(string seriesNameHe, string seriesNameEn, string imageUrl)
    {
        SeriesNameHe = seriesNameHe;
        SeriesNameEn = seriesNameEn;
        SeriesCode = GetSeriesCodeFromImageUrl(imageUrl);
    }

    public SeriesInformation(string seriesFullName, string imageUrl)
    {
        var names = seriesFullName.Split('/');
        SeriesNameHe = names[0].Trim();
        SeriesNameEn = names[1].Trim();
        SeriesCode = GetSeriesCodeFromImageUrl(imageUrl);
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex NumberRegex();

    public static int GetSeriesCodeFromImageUrl(string imageUrl) => int.Parse(NumberRegex().Match(imageUrl).Value);
}
