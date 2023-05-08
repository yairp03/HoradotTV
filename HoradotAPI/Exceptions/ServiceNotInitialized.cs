namespace HoradotAPI.Exceptions;

public class ServiceNotInitialized : InvalidOperationException
{
    public ServiceNotInitialized([CallerMemberName] string serviceName = "") : base(
        $"Service {serviceName} is not initialized.")
    {
    }
}
