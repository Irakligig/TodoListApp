using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

public static class JwtHelper
{
    public static string GenerateDevJwt()
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("YourSuperSecretKey123!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", "dev-user"),
                new Claim(ClaimTypes.NameIdentifier, "dev-user")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "TodoListApp",
            Audience = "TodoListAppUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
