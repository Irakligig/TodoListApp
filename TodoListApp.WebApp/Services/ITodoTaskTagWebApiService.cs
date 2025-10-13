using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoTaskTagWebApiService
{
    Task<List<string>> GetAllTagsAsync();

    Task<List<string>> GetTagsForTaskAsync(int taskId);

    Task AddTagToTaskAsync(int taskId, string tagName);

    Task RemoveTagFromTaskAsync(int taskId, string tagName);

    Task<List<TodoTaskModel>> GetTasksByTagAsync(string tagName);
}
