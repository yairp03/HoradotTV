namespace HoradotAPI.Models;

public record MovieInformation : MediaInformation
{
    public string? ImageUrl { get; init; }
}
