using TodoListApp.Services.Database.Entities;

namespace TodoListApp.WebApi.Services
{
    public interface ITodoCommentDatabaseService
    {
        Task<List<TodoCommentEntity>> GetByTaskIdAsync(int taskId, string userId);

        Task<TodoCommentEntity> AddCommentAsync(int taskId, string userId, string userName, string text);

        Task EditCommentAsync(int commentId, string userId, string newText);

        Task DeleteCommentAsync(int commentId, string userId);
    }
}
