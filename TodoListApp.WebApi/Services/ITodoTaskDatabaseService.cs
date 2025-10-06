using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public interface ITodoTaskDatabaseService
{
    Task<IEnumerable<TodoTask>> GetAllTasksAsync(int todoListId, string ownerId);

    Task<TodoTask?> GetTaskByIdAsync(int taskId, string ownerId);

    Task AddTaskAsync(TodoTask task, string ownerId);

    Task UpdateTaskAsync(TodoTask task, string ownerId);

    Task DeleteTaskAsync(int taskId, string ownerId);

    Task<IEnumerable<TodoTask>> GetAssignedTasksAsync(string userId, string? status = null, string? sortby = null);

    Task UpdateTaskStatusAsync(int taskId, bool isCompleted, string userId);

    Task<TodoTask?> GetTaskByIdForAssignedUserAsync(int taskId, string userId);

    Task ReassignTaskAsync(int taskId, string currentUserId, string newUserId);

    Task<IEnumerable<TodoTask>> SearchTasksAsync(
    string userId,
    string? query,
    bool? status,
    DateTime? dueBefore,
    string? assignedUserId);
}
