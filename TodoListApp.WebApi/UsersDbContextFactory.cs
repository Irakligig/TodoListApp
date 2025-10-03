using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TodoListApp.Services.Database; // where UsersDbContext lives

namespace TodoListApp.WebApi
{
    public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            // Build configuration to read appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // WebApi project folder
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
            var connectionString = configuration.GetConnectionString("UsersDb");

            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("TodoListApp.Services.Database"));

            return new UsersDbContext(optionsBuilder.Options);
        }

        /*
         Commands for running migrations -->
         dotnet ef migrations add CreateUsersTable -c UsersDbContext --project ../TodoListApp.Services.Database --startup-project .
         dotnet ef database update -c UsersDbContext --project ../TodoListApp.Services.Database --startup-project .
         */
    }
}
