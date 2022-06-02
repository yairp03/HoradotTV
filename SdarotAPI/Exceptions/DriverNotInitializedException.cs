namespace SdarotAPI.Exceptions;

internal class DriverNotInitializedException : Exception
{
    public DriverNotInitializedException()
    {
    }

    public DriverNotInitializedException(string? message) : base(message)
    {
    }

    public DriverNotInitializedException(string? message, Exception? inner) : base(message, inner)
    {
    }
}
