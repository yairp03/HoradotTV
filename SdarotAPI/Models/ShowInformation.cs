namespace SdarotAPI.Models;

public class ShowInformation
{
    [JsonPropertyName("heb")] public string NameHe { get; init; } = string.Empty;

    [JsonPropertyName("eng")] public string NameEn { get; init; } = string.Empty;

    [JsonIgnore] public int Code { get; private set; }

    private string? posterName;

    [JsonPropertyName("poster")]
    public string? PosterName
    {
        get => posterName ?? GetDefaultPosterName();
        set => posterName = value;
    }

    [JsonPropertyName("id")]
    public string Id
    {
        get => Code.ToString();
        set => Code = int.Parse(value);
    }

    [JsonIgnore] public string PosterUrl => $"{Constants.SdarotUrls.ImageUrl}{PosterName}";

    [JsonIgnore] public string Url => $"{Constants.SdarotUrls.WatchUrl}{Code}";

    [JsonConstructor]
    public ShowInformation()
    {
    }

    public ShowInformation(string fullName, int code, string? posterName)
    {
        string[] names = fullName.Split('/');
        NameHe = names[0].Trim();
        NameEn = names[1].Trim();
        Code = code;
        PosterName = posterName;
    }

    public ShowInformation(string nameHe, string nameEn, int code, string? posterName = null)
    {
        NameHe = nameHe;
        NameEn = nameEn;
        Code = code;
        PosterName = posterName;
    }

    private string GetDefaultPosterName() => $"{Code}.{Constants.DefaultPosterFormat}";
}
