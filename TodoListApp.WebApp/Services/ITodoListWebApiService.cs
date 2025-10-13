using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public interface ITodoListWebApiService
    {
        // Returns null if JWT is missing or request fails
        Task<IEnumerable<TodoListModel>?> GetTodoListsAsync();

        // Returns false if JWT is missing or request fails
        Task<bool> AddTodoListAsync(TodoListModel newList);

        Task<bool> UpdateTodoListAsync(TodoListModel updatedList);

        Task<bool> DeleteTodoListAsync(int id);
    }
}
