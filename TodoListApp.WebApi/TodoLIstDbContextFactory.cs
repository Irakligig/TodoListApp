using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TodoListApp.Services.Database;

namespace TodoListApp.WebApi
{
    public class TodoListDbContextFactory : IDesignTimeDbContextFactory<TodoListDbContext>
    {
        public TodoListDbContext CreateDbContext(string[] args)
        {
            // Build configuration to read appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // WebApi project folder
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TodoListDbContext>();
            var connectionString = configuration.GetConnectionString("TodoListDb");

            _ = optionsBuilder.UseSqlServer(connectionString);

            return new TodoListDbContext(optionsBuilder.Options);
        }
    }
}
