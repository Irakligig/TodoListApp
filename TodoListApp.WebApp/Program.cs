using Microsoft.AspNetCore.Authentication.Cookies;
using TodoListApp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// Add services to the container
// ---------------------------

// Controllers + Views
builder.Services.AddControllersWithViews();

// HttpContextAccessor (needed for reading cookies)
builder.Services.AddHttpContextAccessor();

// Auth service (per HTTP request, scoped)
builder.Services.AddScoped<IUsersAuthWebApiService, UsersAuthWebApiService>();

// ---------------------------
// Register WebAPI services (also scoped)
// ---------------------------
builder.Services.AddScoped<ITodoListWebApiService, TodoListWebApiService>();
builder.Services.AddScoped<ITodoTaskWebApiService, TodoTaskWebApiService>();
builder.Services.AddScoped<ITodoTaskTagWebApiService, TodoTaskTagWebApiService>();
builder.Services.AddScoped<ITodoCommentWebApiService, TodoCommentWebApiService>();

// ---------------------------
// Configure a shared HttpClient for all services
// ---------------------------
builder.Services.AddHttpClient("WebApiClient", client =>
{
    client.BaseAddress = apiBase;
});

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";   // redirect here if not authenticated
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// ---------------------------
// Build app
// ---------------------------
var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "search",
    pattern: "Search/{action=Index}/{id?}",
    defaults: new { controller = "Search" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TodoList}/{action=Index}/{id?}");

app.Run();
