using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
namespace TodoListApp.WebApi.Services;

public class TodoCommentDatabaseService : ITodoCommentDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ITodoTaskDatabaseService taskService;

    public TodoCommentDatabaseService(TodoListDbContext db, ITodoTaskDatabaseService taskService)
    {
        this.context = db;
        this.taskService = taskService;
    }

    private static TodoCommentEntity MapEntityToModel(TodoCommentEntity entity)
    {
        return new TodoCommentEntity
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            UserId = entity.UserId,
            Text = entity.Text,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<TodoCommentEntity>> GetByTaskIdAsync(int taskId, string userId)
    {
        var task = await taskService.GetTaskByIdAsync(taskId, userId);
        if (task == null)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        var comments = await context.Comments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapEntityToModel).ToList();
    }

    public async Task<TodoCommentEntity> AddCommentAsync(int taskId, string userId, string text)
    {
        var task = await taskService.GetTaskByIdAsync(taskId, userId);
        if (task == null)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        var entity = new TodoCommentEntity
        {
            TaskId = taskId,
            UserId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        context.Comments.Add(entity);
        await context.SaveChangesAsync();

        return MapEntityToModel(entity);
    }

    public async Task EditCommentAsync(int commentId, string userId, string newText)
    {
        var entity = await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            throw new Exception("Comment not found.");
        }

        var task = await taskService.GetTaskByIdAsync(entity.TaskId, userId);
        if (task == null)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        if (entity.UserId != userId && task.AssignedUserId != userId && task.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You cannot edit this comment.");
        }

        entity.Text = newText;
        await context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int commentId, string userId)
    {
        var entity = await context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (entity == null)
        {
            throw new Exception("Comment not found.");
        }

        var task = await taskService.GetTaskByIdAsync(entity.TaskId, userId);
        if (task == null)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        if (entity.UserId != userId && task.AssignedUserId != userId && task.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("You cannot delete this comment.");
        }

        context.Comments.Remove(entity);
        await context.SaveChangesAsync();
    }
}
