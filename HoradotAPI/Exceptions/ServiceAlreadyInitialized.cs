namespace HoradotAPI.Exceptions;

public class ServiceAlreadyInitialized : InvalidOperationException
{
    public ServiceAlreadyInitialized([CallerMemberName] string serviceName = "") : base(
        $"Service {serviceName} is already initialized.")
    {
    }
}
