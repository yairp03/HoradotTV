namespace HoradotAPI.Models;

[JsonDerivedType(typeof(ShowInformation), "base")]
[JsonDerivedType(typeof(SdarotTVShowInformation), "sdarotTVShow")]
public record ShowInformation : MediaInformation
{
    public string? ImageUrl { get; set; }

    public override string ToString() => base.ToString();
}
