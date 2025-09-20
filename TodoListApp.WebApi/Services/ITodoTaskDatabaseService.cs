using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public interface ITodoTaskDatabaseService
{
    Task<IEnumerable<TodoTask>> GetAllTasksAsync(int todoListId, string ownerId);

    Task<TodoTask?> GetTaskByIdAsync(int taskId, string ownerId);

    Task AddTaskAsync(TodoTask task, string ownerId);

    Task UpdateTaskAsync(TodoTask task, string ownerId);

    Task DeleteTaskAsync(int taskId, string ownerId);
}
