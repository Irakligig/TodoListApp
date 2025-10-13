using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TodoListApp.WebApp.Helpers;
using TodoListApp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Needed to access cookies inside services
builder.Services.AddHttpContextAccessor();

var apiBase = new Uri("https://localhost:7001/");

// Register HttpClients
builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>(client =>
{
    client.BaseAddress = apiBase;
});

builder.Services.AddHttpClient<ITodoTaskWebApiService, TodoTaskWebApiService>(client =>
{
    client.BaseAddress = apiBase;
});

builder.Services.AddHttpClient<ITodoTaskTagWebApiService, TodoTaskTagWebApiService>(client =>
{
    client.BaseAddress = apiBase;
});

builder.Services.AddHttpClient<ITodoCommentWebApiService, TodoCommentWebApiService>(client =>
{
    client.BaseAddress = apiBase;
});

// Auth service singleton
builder.Services.AddSingleton<IUsersAuthWebApiService>(sp =>
{
    var http = new HttpClient { BaseAddress = new Uri("https://localhost:7001/") };
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    return new UsersAuthWebApiService(http, httpContextAccessor);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
})
.AddJwtBearer(options =>
{
    options.Authority = "https://localhost:7001/"; // your API base
    options.Audience = "TodoListApi";
    options.RequireHttpsMetadata = true;
});


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
app.UseCookiePolicy();
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
