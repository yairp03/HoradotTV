namespace SdarotAPI.Exceptions;

public class SdarotBlockedException : Exception
{
    public SdarotBlockedException() : base("The SdarotTV site is blocked.")
    {
    }
}
