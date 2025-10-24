using TodoListApp.Services.Database.Entities;

namespace TodoListApp.WebApi.Services
{
    public interface ITodoCommentDatabaseService
    {
        // Get all comments for a given task (checks access)
        Task<List<TodoCommentEntity>> GetByTaskIdAsync(int taskId, string userId);

        // Add a new comment to a task
        Task<TodoCommentEntity> AddCommentAsync(int taskId, string userId, string userName, string text);

        // Edit an existing comment (author or owner only)
        Task EditCommentAsync(int commentId, string userId, string newText);

        // Delete a comment (author or owner only)
        Task DeleteCommentAsync(int commentId, string userId);
    }
}
