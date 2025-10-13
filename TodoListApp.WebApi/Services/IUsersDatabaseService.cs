using TodoListApp.Services.Database.Entities;

namespace TodoListApp.WebApi.Services
{
    public interface IUsersDatabaseService
    {
        Task<User?> GetByIdAsync(string id);

        Task<IEnumerable<User>> GetAllAsync();
    }
}
