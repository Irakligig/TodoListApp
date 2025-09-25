using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoListWebApiService
{
    Task<IEnumerable<TodoListModel>> GetTodoListsAsync();

    Task<TodoListModel?> AddTodoListAsync(TodoListModel newList);

    Task<bool> DeleteTodoListAsync(int id);

    Task<TodoListModel?> UpdateTodoListAsync(TodoListModel updatedList);
}
