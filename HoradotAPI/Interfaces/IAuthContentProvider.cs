namespace HoradotAPI.Interfaces;

public interface IAuthContentProvider : IContentProvider
{
    public new bool LoginRequired => true;

    public Task<bool> IsLoggedIn();
    public Task<bool> Login(string username, string password);
}
