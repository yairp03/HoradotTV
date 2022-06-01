namespace SdarotAPI.Exceptions;

internal class DriverAlreadyInitializedException : Exception
{
    public DriverAlreadyInitializedException()
    {
    }

    public DriverAlreadyInitializedException(string? message) : base(message)
    {
    }

    public DriverAlreadyInitializedException(string? message, Exception? inner) : base(message, inner)
    {
    }
}
