namespace TodoListApp.WebApp.Services;

public interface IUsersAuthWebApiService
{
    string? CurrentUserId { get; }

    string? JwtToken { get; }

    Task<bool> RegisterAsync(string username, string email, string password, string fullName);

    Task<bool> LoginAsync(string username, string password);

    void Logout();
}
