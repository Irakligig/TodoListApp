using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;

namespace TodoListApp.WebApi;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // added db context to webapi project
        _ = builder.Services.AddDbContext<TodoListDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDb")));
        _ = builder.Services.AddControllers();
        var app = builder.Build();
        _ = app.MapControllers();
        app.Run();
    }
}
