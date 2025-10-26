using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace TodoListApp.WebApp.Services
{
    public class UsersAuthWebApiService : IUsersAuthWebApiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UsersAuthWebApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public string? JwtToken
        {
            get
            {
                return this.httpContextAccessor.HttpContext?.Request.Cookies["JwtToken"];
            }
        }

        public bool IsJwtPresent()
        {
            if (string.IsNullOrEmpty(this.JwtToken))
            {
                return false;
            }

            return true;
        }

        public string? CurrentUserId => GetUserIdFromJwt(JwtToken);

        public string? CurrentUserName => GetUserNameFromJwt(JwtToken);

        public async Task<bool> RegisterAsync(string username, string email, string password, string fullName)
        {
            var client = this.httpClientFactory.CreateClient("WebApiClient");
            var model = new { Username = username, Email = email, Password = password, FullName = fullName };
            var response = await client.PostAsJsonAsync("api/auth/register", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var client = this.httpClientFactory.CreateClient("WebApiClient");
            Console.WriteLine($"Calling API login for {username}");

            var response = await client.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });
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

            // Read the actual JWT token to get its exact expiration
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(result.Token);
            var tokenExpiration = jwtToken.ValidTo;

            // Local HTTPS check: only set Secure if HTTPS
            var isHttps = this.httpContextAccessor.HttpContext?.Request.IsHttps ?? false;
            Console.WriteLine("Request IsHttps: " + isHttps);

            this.httpContextAccessor.HttpContext?.Response.Cookies.Append(
                "JwtToken",
                result.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = tokenExpiration,
                });

            Console.WriteLine("JWT cookie set: " + result.Token);

            return true;
        }

        public void Logout()
        {
            this.httpContextAccessor.HttpContext?.Response.Cookies.Delete("JwtToken");
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

        private string? GetUserNameFromJwt(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }


            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Your backend uses ClaimTypes.Name for the username
            var userName = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            Console.WriteLine($"JWT Username extraction: Found '{userName}'");
            return userName;

        }

        public bool IsJwtValid()
        {
            if (string.IsNullOrEmpty(this.JwtToken))
            {
                Console.WriteLine("JWT token is null or empty");
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(this.JwtToken);
                var isValid = jwt.ValidTo > DateTime.UtcNow;

                Console.WriteLine($"Token validation: IsValid={isValid}, Expires={jwt.ValidTo}, CurrentTime={DateTime.UtcNow}");

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating JWT: {ex.Message}");
                throw;
            }
        }

        private sealed class LoginResponse
        {
            public string Token { get; set; } = default!;
        }
    }
}
