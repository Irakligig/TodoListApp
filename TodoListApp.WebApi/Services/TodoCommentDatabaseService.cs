using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
namespace TodoListApp.WebApi.Services;

public class TodoCommentDatabaseService : ITodoCommentDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ITodoTaskDatabaseService taskService;
    private readonly ILogger<TodoCommentDatabaseService> logger;

    public TodoCommentDatabaseService(TodoListDbContext db, ITodoTaskDatabaseService taskService, ILogger<TodoCommentDatabaseService> logger)
    {
        this.context = db;
        this.taskService = taskService;
        this.logger = logger;
    }

    private static TodoCommentEntity MapEntityToModel(TodoCommentEntity entity)
    {
        return new TodoCommentEntity
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            UserId = entity.UserId,
            UserName = entity.UserName, // Add this line
            Text = entity.Text,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<TodoCommentEntity>> GetByTaskIdAsync(int taskId, string userId)
    {
        logger.LogInformation("Getting comments for task {TaskId} for user {UserId}", taskId, userId);

        var task = await taskService.GetTaskByIdAsync(taskId, userId);
        if (task == null)
        {
            logger.LogWarning("User {UserId} does not have access to task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        var comments = await context.Comments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {CommentCount} comments for task {TaskId}", comments.Count, taskId);
        return comments.Select(MapEntityToModel).ToList();
    }

    public async Task<TodoCommentEntity> AddCommentAsync(int taskId, string userId, string userName, string text)
    {
        logger.LogInformation("Adding comment to task {TaskId} by user {UserId} ({UserName})", taskId, userId, userName);

        var task = await taskService.GetTaskByIdAsync(taskId, userId);
        if (task == null)
        {
            logger.LogWarning("User {UserId} does not have access to add comment to task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        var entity = new TodoCommentEntity
        {
            TaskId = taskId,
            UserId = userId,
            UserName = userName, // Save the username
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        context.Comments.Add(entity);
        await context.SaveChangesAsync();

        logger.LogInformation("Successfully added comment {CommentId} to task {TaskId} by user {UserName}", entity.Id, taskId, userName);
        return MapEntityToModel(entity);
    }

    public async Task EditCommentAsync(int commentId, string userId, string newText)
    {
        logger.LogInformation("Editing comment {CommentId} by user {UserId}", commentId, userId);

        var entity = await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            logger.LogWarning("Comment {CommentId} not found", commentId);
            throw new Exception("Comment not found.");
        }

        var task = await taskService.GetTaskByIdAsync(entity.TaskId, userId);
        if (task == null)
        {
            logger.LogWarning("User {UserId} does not have access to edit comment {CommentId}", userId, commentId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        if (entity.UserId != userId && task.AssignedUserId != userId && task.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} is not authorized to edit comment {CommentId}", userId, commentId);
            throw new UnauthorizedAccessException("You cannot edit this comment.");
        }

        entity.Text = newText;
        await context.SaveChangesAsync();
        logger.LogInformation("Successfully edited comment {CommentId}", commentId);
    }

    public async Task DeleteCommentAsync(int commentId, string userId)
    {
        logger.LogInformation("Deleting comment {CommentId} by user {UserId}", commentId, userId);

        var entity = await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            logger.LogWarning("Comment {CommentId} not found", commentId);
            throw new Exception("Comment not found.");
        }

        var task = await taskService.GetTaskByIdAsync(entity.TaskId, userId);
        if (task == null)
        {
            logger.LogWarning("User {UserId} does not have access to delete comment {CommentId}", userId, commentId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        // ONLY allow task owners to delete comments
        if (task.OwnerId != userId)
        {
            logger.LogWarning("User {UserId} is not authorized to delete comment {CommentId}. Only task owners can delete comments.", userId, commentId);
            throw new UnauthorizedAccessException("Only the task owner can delete comments.");
        }

        context.Comments.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Successfully deleted comment {CommentId} by task owner {UserId}", commentId, userId);
    }
}
