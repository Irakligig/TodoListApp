using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public interface ITodoListDatabaseService
{
    Task<IEnumerable<TodoList>> GetAllTodoListsAsync(string ownerId);

    Task AddTodoListAsync(TodoList todoList, string ownerId);

    Task DeleteTodoListAsync(int todoListId, string ownerId);

    Task UpdateTodoListAsync(TodoList todoList, string ownerId);
}
