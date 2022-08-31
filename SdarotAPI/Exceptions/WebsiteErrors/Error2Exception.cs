namespace SdarotAPI.Exceptions.WebsiteErrors;

public class Error2Exception : Exception
{
    public Error2Exception()
    {
    }

    public Error2Exception(string? message) : base(message)
    {
    }

    public Error2Exception(string? message, Exception? inner) : base(message, inner)
    {
    }
}
