using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoListWebApiService
{
    Task<IEnumerable<TodoListModel>> GetTodoListsAsync();

    Task AddTodoListAsync(TodoListModel newList);

    Task UpdateTodoListAsync(TodoListModel updatedList);

    Task DeleteTodoListAsync(int id);
}
