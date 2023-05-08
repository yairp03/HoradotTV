namespace HoradotAPI.Models;

[JsonDerivedType(typeof(MovieInformation), "base")]
[JsonDerivedType(typeof(SratimTVMovieInformation), "sratimTVMovie")]
public record MovieInformation : MediaInformation
{
    public string? ImageUrl { get; set; }
}
