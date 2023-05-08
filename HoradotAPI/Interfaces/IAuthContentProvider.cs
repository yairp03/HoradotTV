namespace HoradotAPI.Interfaces;

public interface IAuthContentProvider : IContentProvider
{
    public Task<bool> IsLoggedIn();
    public Task<bool> Login(string username, string password);
}
