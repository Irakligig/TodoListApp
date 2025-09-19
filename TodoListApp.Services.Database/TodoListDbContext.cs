using Microsoft.EntityFrameworkCore;
using TodoListApp.WebApi.Models;

namespace TodoListApp.Services.Database
{
    public class TodoListDbContext : DbContext
    {
        public TodoListDbContext(DbContextOptions<TodoListDbContext> options)
            : base(options)
        {
        }
    }
}
