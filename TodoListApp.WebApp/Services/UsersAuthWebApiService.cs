using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TodoListApp.WebApp.Services
{
    public class UsersAuthWebApiService : IUsersAuthWebApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersAuthWebApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public string? JwtToken
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Request.Cookies["JwtToken"];
            }
        }

        public string? CurrentUserId => GetUserIdFromJwt(JwtToken);

        public async Task<bool> RegisterAsync(string username, string email, string password, string fullName)
        {
            var model = new { Username = username, Email = email, Password = password, FullName = fullName };
            var response = await _http.PostAsJsonAsync("api/auth/register", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            Console.WriteLine($"Calling API login for {username}");

            var response = await _http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });
            Console.WriteLine("API response status: " + response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("API login failed");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API response body: " + json);

            var result = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Token == null)
            {
                Console.WriteLine("JWT token missing in API response");
                return false;
            }

            // Local HTTPS check: only set Secure if HTTPS
            var isHttps = _httpContextAccessor.HttpContext?.Request.IsHttps ?? false;
            Console.WriteLine("Request IsHttps: " + isHttps);

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "JwtToken",
                result.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

            Console.WriteLine("JWT cookie set: " + result.Token);

            return true;
        }


        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete("JwtToken");
        }

        private string? GetUserIdFromJwt(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        private class LoginResponse
        {
            public string Token { get; set; } = default!;
        }
    }
}
