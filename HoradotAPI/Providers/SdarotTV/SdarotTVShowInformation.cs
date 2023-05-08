namespace HoradotAPI.Providers.SdarotTV;

public record SdarotTVShowInformation : ShowInformation
{
    public SdarotTVShowInformation()
    {
        ProviderName = "SdarotTV";
    }

    [JsonPropertyName("heb")]
    public string Heb
    {
        init => NameHe = value;
    }

    [JsonPropertyName("eng")]
    public string Eng
    {
        init => Name = value;
    }

    [JsonPropertyName("id")]
    public string StrId
    {
        init => Id = int.Parse(value);
    }

    [JsonPropertyName("poster")] public string ImageName { get; init; } = string.Empty;

    [JsonPropertyName("year")] public string Year { get; init; } = string.Empty;

    public override string ToString() => base.ToString();
}
