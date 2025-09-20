using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // added db context to webapi project
        _ = builder.Services.AddDbContext<TodoListDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDb")));

        // addscoped because we want new instance for every http request
        _ = builder.Services.AddScoped<ITodoListDatabaseService, TodoListDatabaseService>();

        _ = builder.Services.AddControllers();

        var app = builder.Build();

        _ = app.MapControllers();

        app.Run();
    }
}
