using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoListApp.Services.Database;
using TodoListApp.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext & Services
builder.Services.AddDbContext<TodoListDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDb")));
builder.Services.AddScoped<ITodoListDatabaseService, TodoListDatabaseService>();
builder.Services.AddControllers();

// CORS for Swagger / local testing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Middleware order
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

// Inject a fake user for testing
app.Use(async (context, next) =>
{
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "dev-key") };
    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Dev"));
    await next();
});

app.UseAuthorization(); // still needed for [Authorize] attributes

// Map controllers
app.MapControllers();

// Swagger (optional)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API V1");
    c.RoutePrefix = string.Empty;
});

app.Run();
