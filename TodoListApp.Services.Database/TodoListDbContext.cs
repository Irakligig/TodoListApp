using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;

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

        public DbSet<TodoTagEntity> TodoTags => this.Set<TodoTagEntity>();

        public DbSet<TodoTaskTagEntity> TodoTaskTags => this.Set<TodoTaskTagEntity>();

        public DbSet<TodoCommentEntity> Comments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<TodoListEntity>()
                .HasMany(tl => tl.TodoItems)
                .WithOne(t => t.TodoList)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            _ = modelBuilder.Entity<TodoTaskTagEntity>()
        .HasKey(tt => new { tt.TodoTaskId, tt.TagId });

            _ = modelBuilder.Entity<TodoTaskTagEntity>()
                .HasOne(tt => tt.TodoTask) // from todotasktagperspective
                .WithMany(t => t.TaskTags) // from todotaskperspective
                .HasForeignKey(tt => tt.TodoTaskId);

            _ = modelBuilder.Entity<TodoTaskTagEntity>()
                .HasOne(tt => tt.Tag) // from todotasktagperspective
                .WithMany(t => t.TaskTags) // from todotagperspective
                .HasForeignKey(tt => tt.TagId);
        }
    }
}
