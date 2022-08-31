namespace SdarotAPI.Exceptions.WebsiteErrors;

public class WebsiteErrorException : Exception
{
    public WebsiteErrorException()
    {
    }

    public WebsiteErrorException(string? message) : base(message)
    {
    }

    public WebsiteErrorException(string? message, Exception? inner) : base(message, inner)
    {
    }
}
