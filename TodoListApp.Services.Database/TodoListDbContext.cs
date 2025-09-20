using Microsoft.EntityFrameworkCore;

namespace TodoListApp.Services.Database
{
    public class TodoListDbContext : DbContext
    {
        public TodoListDbContext(DbContextOptions<TodoListDbContext> options)
            : base(options)
        {
        }

        public DbSet<TodoListEntity> TodoLists => this.Set<TodoListEntity>();

        public DbSet<TodoTaskEntity> TodoTasks => this.Set<TodoTaskEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TodoListEntity>()
                .HasMany(tl => tl.TodoItems)
                .WithOne(t => t.TodoList)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
