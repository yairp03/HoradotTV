namespace HoradotAPI.Exceptions;

public class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException(string serviceName) : base($"Not logged in to {serviceName}")
    {
    }
}
