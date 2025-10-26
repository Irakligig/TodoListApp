using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Controllers;

[AllowAnonymous]
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly IConfiguration config;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration config)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        var user = new User { UserName = model.Username, Email = model.Email, FullName = model.FullName};
        var result = await this.userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return this.BadRequest(result.Errors);
        }

        return this.Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await this.userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return this.Unauthorized();
        }

        var result = await this.signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return this.Unauthorized();
        }

        await this.signInManager.SignInAsync(user, isPersistent: false);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: this.config["Jwt:Issuer"],
            audience: this.config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(this.config["Jwt:DurationInMinutes"])),
            signingCredentials: creds
        );

        return this.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
