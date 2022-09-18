namespace SdarotAPI.Exceptions;

internal class ChromeIsNotInstalledException : Exception
{
    public ChromeIsNotInstalledException() : base("Chrome is not installed.")
    {
    }

    public ChromeIsNotInstalledException(string? message) : base(message)
    {
    }

    public ChromeIsNotInstalledException(string? message, Exception? inner) : base(message, inner)
    {
    }
}