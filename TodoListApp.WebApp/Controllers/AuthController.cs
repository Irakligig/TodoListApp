using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUsersAuthWebApiService _authService;

        public AuthController(IUsersAuthWebApiService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _authService.RegisterAsync(model.Username, model.Email, model.Password, model.FullName);

            if (success)
            {
                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", "Registration failed. Try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            Console.WriteLine("Login POST called");
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                Console.WriteLine("ModelState invalid: " + errors);
                return View(model);
            }

            var success = await _authService.LoginAsync(model.Username, model.Password);
            Console.WriteLine("API login success: " + success);

            if (success)
            {
                var userId = _authService.CurrentUserId;
                Console.WriteLine("UserId from JWT: " + userId);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.NameIdentifier, userId ?? "")
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                Console.WriteLine("SignInAsync completed");
                Console.WriteLine("User.Identity.IsAuthenticated: " + HttpContext.User.Identity?.IsAuthenticated);
                return RedirectToAction("Index", "TodoList");
            }
            else
            {
                ModelState.AddModelError("", "Invalid username or password.");
                Console.WriteLine("Login failed");
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authService.Logout();
            return RedirectToAction("Login");
        }
    }
}
