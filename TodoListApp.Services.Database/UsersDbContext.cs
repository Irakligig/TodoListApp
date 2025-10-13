using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.Services.Database;
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<User>().HasData(
            new User { Id = "1", Email = "admin@example.com", FullName = "Admin User" },
            new User { Id = "2", Email = "user1@example.com", FullName = "User One" },
            new User { Id = "3", Email = "user2@example.com", FullName = "User Two" });
    }
}
