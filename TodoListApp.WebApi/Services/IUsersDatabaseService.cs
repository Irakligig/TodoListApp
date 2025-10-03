using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;

namespace TodoListApp.WebApi.Services
{
    public interface IUsersDatabaseService
    {
        Task<User?> GetByIdAsync(string id);

        Task<IEnumerable<User>> GetAllAsync();
    }
}
