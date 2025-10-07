using TodoListApp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register ITodoListWebApiService with HttpClient
builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/"); // Web API base URL
});

// Register ITodoTaskWebApiService with HttpClient
builder.Services.AddHttpClient<ITodoTaskWebApiService, TodoTaskWebApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
});
// Register ITodoTaskTagWebApiService with HttpClient
builder.Services.AddHttpClient<ITodoTaskTagWebApiService, TodoTaskTagWebApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
});

// Register ITodoCommentWebApiService with HttpClient
builder.Services.AddHttpClient<ITodoCommentWebApiService, TodoCommentWebApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/");
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- AUTH: here you can use cookie auth if needed ---
app.UseAuthorization();

app.MapControllerRoute(
    name: "search",
    pattern: "Search/{action=Index}/{id?}",
    defaults: new { controller = "Search" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TodoList}/{action=Index}/{id?}");

app.Run();
