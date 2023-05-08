namespace HoradotAPI.Exceptions.WebsiteErrors;

public class ElementNotFoundException : WebsiteErrorException
{
    public ElementNotFoundException(string? element) : base(
        $"The {element} element was not found. Please contact developer.")
    {
    }
}
