using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public interface ITodoCommentWebApiService
{
    Task<IEnumerable<TodoCommentModel>> GetCommentsAsync(int taskId);

    Task AddCommentAsync(int taskId, string text);

    Task EditCommentAsync(int taskId, int commentId, string newText);

    Task DeleteCommentAsync(int taskId, int commentId);
}
