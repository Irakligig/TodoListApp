using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public interface ITodoTaskTagDatabaseService
{
    // Add a tag to a task
    Task AddTagToTaskAsync(int taskId, string tagName, string userId);

    // Remove a tag from a task
    Task RemoveTagFromTaskAsync(int taskId, string tagName, string userId);

    // Get all tags for the current user
    Task<List<string>> GetAllTagsAsync(string userId);

    // Get tasks filtered by a tag
    Task<IEnumerable<TodoTask>> GetTasksByTagAsync(string userId, string tagName);

    // NEW: Get a task along with its tags and list ID
    Task<TaskWithTagsViewModel> GetTaskWithTagsAsync(int taskId, string userId);

    Task<List<string>> GetTagsForTaskAsync(int taskId, string userId);
}
