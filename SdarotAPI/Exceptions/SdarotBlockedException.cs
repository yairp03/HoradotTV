namespace SdarotAPI.Exceptions;

internal class SdarotBlockedException : Exception
{
    public SdarotBlockedException() : base("The SdarotTV site is blocked.")
    {
    }

    public SdarotBlockedException(string? message) : base(message)
    {
    }

    public SdarotBlockedException(string? message, Exception? inner) : base(message, inner)
    {
    }
}