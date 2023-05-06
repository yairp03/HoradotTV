namespace SdarotAPI.Exceptions.WebsiteErrors;

public class WebsiteErrorException : Exception
{
    public WebsiteErrorException(string? message) : base(message)
    {
    }
}
