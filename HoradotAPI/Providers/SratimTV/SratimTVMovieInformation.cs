namespace HoradotAPI.Providers.SratimTV;

public record SratimTVMovieInformation : MovieInformation
{
    public SratimTVMovieInformation()
    {
        ProviderName = "SratimTV";
    }

    [JsonPropertyName("id")]
    public string StrId
    {
        init => Id = int.Parse(value);
    }

    [JsonPropertyName("name")]
    public string FullName
    {
        init
        {
            string[] names = value.Split("/");
            NameHe = names[0].Trim();
            Name = names[1].Trim();
        }
    }

    public override string ToString() => base.ToString();
}
