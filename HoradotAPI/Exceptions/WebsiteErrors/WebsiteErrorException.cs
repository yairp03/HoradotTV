namespace HoradotAPI.Exceptions.WebsiteErrors;

public class WebsiteErrorException : Exception
{
    protected WebsiteErrorException(string? message) : base(message)
    {
    }
}
