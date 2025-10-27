using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.WebApi.Services;

public class TodoCommentDatabaseService : ITodoCommentDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ITodoTaskDatabaseService taskService;
    private readonly ILogger<TodoCommentDatabaseService> logger;
    private readonly IPermissionService permissionService;

    public TodoCommentDatabaseService(TodoListDbContext db, ITodoTaskDatabaseService taskService, ILogger<TodoCommentDatabaseService> logger,IPermissionService permissionService)
    {
        this.context = db;
        this.taskService = taskService;
        this.logger = logger;
        this.permissionService = permissionService;
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
            CreatedAt = entity.CreatedAt,
        };
    }

    public async Task<List<TodoCommentEntity>> GetByTaskIdAsync(int taskId, string userId)
    {
        this.logger.LogInformation("Getting comments for task {TaskId} for user {UserId}", taskId, userId);

        if (!await this.permissionService.CanViewTaskAsync(taskId, userId))
        {
            this.logger.LogWarning("User {UserId} does not have access to task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        var comments = await this.context.Comments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        this.logger.LogInformation("Retrieved {CommentCount} comments for task {TaskId}", comments.Count, taskId);
        return comments.Select(MapEntityToModel).ToList();
    }

    public async Task<TodoCommentEntity> AddCommentAsync(int taskId, string userId, string userName, string text)
    {
        this.logger.LogInformation("Adding comment to task {TaskId} by user {UserId} ({UserName})", taskId, userId, userName);

        if (!await this.permissionService.CanManageCommentsAsync(taskId, userId))
        {
            this.logger.LogWarning("User {UserId} does not have permission to add comment to task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You do not have permission to add comments to this task.");
        }

        var entity = new TodoCommentEntity
        {
            TaskId = taskId,
            UserId = userId,
            UserName = userName, // Save the username
            Text = text,
            CreatedAt = DateTime.UtcNow,
        };

        _ = this.context.Comments.Add(entity);
        _ = await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully added comment {CommentId} to task {TaskId} by user {UserName}", entity.Id, taskId, userName);
        return MapEntityToModel(entity);
    }

    public async Task EditCommentAsync(int commentId, string userId, string newText)
    {
        this.logger.LogInformation("Editing comment {CommentId} by user {UserId}", commentId, userId);

        var entity = await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            this.logger.LogWarning("Comment {CommentId} not found", commentId);
            throw new Exception("Comment not found.");
        }

        if (!await permissionService.CanManageCommentsAsync(entity.TaskId, userId))
        {
            this.logger.LogWarning("User {UserId} does not have permission to edit comments for task {TaskId}", userId, entity.TaskId);
            throw new UnauthorizedAccessException("You do not have permission to edit comments for this task.");
        }

        // Additional check: Users can only edit their own comments
        if (entity.UserId != userId)
        {
            this.logger.LogWarning("User {UserId} is not authorized to edit comment {CommentId} created by user {CommentUserId}",
                userId, commentId, entity.UserId);
            throw new UnauthorizedAccessException("You can only edit your own comments.");
        }

        entity.Text = newText;
        _ = await this.context.SaveChangesAsync();
        this.logger.LogInformation("Successfully edited comment {CommentId}", commentId);
    }

    public async Task DeleteCommentAsync(int commentId, string userId)
    {
        this.logger.LogInformation("Deleting comment {CommentId} by user {UserId}", commentId, userId);

        var entity = await this.context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            this.logger.LogWarning("Comment {CommentId} not found", commentId);
            throw new KeyNotFoundException($"Comment {commentId} not found."); // Use KeyNotFoundException
        }

        // Check if user has permission to manage comments for this task
        if (!await this.permissionService.CanManageCommentsAsync(entity.TaskId, userId))
        {
            this.logger.LogWarning("User {UserId} does not have permission to delete comments for task {TaskId}", userId, entity.TaskId);
            throw new UnauthorizedAccessException("You do not have permission to delete comments for this task.");
        }

        // Additional check: Users can only delete their own comments unless they have higher privileges
        // Comment authors can delete their own comments
        // Owners/Editors can delete any comments in tasks they manage
        bool isCommentAuthor = entity.UserId == userId;
        bool canManageTasks = await permissionService.CanManageTasksAsync(
            (await context.TodoTasks.FirstOrDefaultAsync(t => t.Id == entity.TaskId))?.TodoListId ?? 0,
            userId);

        if (!isCommentAuthor && !canManageTasks)
        {
            this.logger.LogWarning("User {UserId} is not authorized to delete comment {CommentId} created by user {CommentUserId}",
                userId, commentId, entity.UserId);
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }

        _ = this.context.Comments.Remove(entity);
        _ = await this.context.SaveChangesAsync();
        this.logger.LogInformation("Successfully deleted comment {CommentId} by user {UserId}", commentId, userId);
    }
}
