using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public interface ITodoTaskTagDatabaseService
{
    Task AddTagToTaskAsync(int taskId, string tagName, string userId);

    Task RemoveTagFromTaskAsync(int taskId, string tagName, string userId);

    Task<List<string>> GetAllTagsAsync(string userId);

    Task<IEnumerable<TodoTask>> GetTasksByTagAsync(string userId, string tagName);

    Task<TaskWithTagsViewModel> GetTaskWithTagsAsync(int taskId, string userId);

    Task<List<string>> GetTagsForTaskAsync(int taskId, string userId);
}
