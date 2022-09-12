namespace SdarotAPI.Exceptions.WebsiteErrors;

public class ElementNotFoundException : WebsiteErrorException
{
    public ElementNotFoundException()
    {
    }

    public ElementNotFoundException(string? element) : base($"The {element} element was not found. Please contact developer.")
    {
    }

    public ElementNotFoundException(string? message, Exception? inner) : base(message, inner)
    {
    }
}
