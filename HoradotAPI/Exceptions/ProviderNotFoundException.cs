namespace HoradotAPI.Exceptions;

public class ProviderNotFoundException : Exception
{
    public ProviderNotFoundException(string providerName) : base($"Provider {providerName} not found.")
    {
    }
}
