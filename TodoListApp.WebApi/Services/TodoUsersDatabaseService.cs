using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.WebApi.Services;

public class TodoUsersDatabaseService : IUsersDatabaseService
{
    private readonly UsersDbContext _context;

    public TodoUsersDatabaseService(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }
}
