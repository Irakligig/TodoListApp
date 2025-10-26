using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public class TodoTaskTagDatabaseService : ITodoTaskTagDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ILogger<TodoTaskTagDatabaseService> logger;

    public TodoTaskTagDatabaseService(TodoListDbContext context, ILogger<TodoTaskTagDatabaseService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    // ---------------- Add Tag ----------------
    public async Task AddTagToTaskAsync(int taskId, string tagName, string userId)
    {
        this.logger.LogInformation("Adding tag '{TagName}' to task {TaskId} for user {UserId}", tagName, taskId, userId);

        var task = await this.context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            this.logger.LogWarning("Task {TaskId} not found or access denied for user {UserId}", taskId, userId);
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

        var normalizedTagName = tagName.ToLower();
        var tag = await this.context.TodoTags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedTagName);

        if (tag == null)
        {
            this.logger.LogInformation("Creating new tag '{TagName}'", tagName);
            tag = new TodoTagEntity { Name = tagName };
            _ = await this.context.TodoTags.AddAsync(tag);
            _ = await this.context.SaveChangesAsync();
        }

        if (!task.TaskTags.Any(tt => tt.TagId == tag.Id))
        {
            this.logger.LogInformation("Associating tag '{TagName}' (ID: {TagId}) with task {TaskId}", tagName, tag.Id, taskId);
            task.TaskTags.Add(new TodoTaskTagEntity
            {
                TodoTaskId = taskId,
                TagId = tag.Id,
            });
            _ = await this.context.SaveChangesAsync();
            this.logger.LogInformation("Successfully added tag '{TagName}' to task {TaskId}", tagName, taskId);
        }
        else
        {
            this.logger.LogInformation("Tag '{TagName}' already exists on task {TaskId}", tagName, taskId);
        }
    }

    // ---------------- Remove Tag ----------------
    public async Task RemoveTagFromTaskAsync(int taskId, string tagName, string userId)
    {
        this.logger.LogInformation("Removing tag '{TagName}' from task {TaskId} for user {UserId}", tagName, taskId, userId);

        var task = await this.context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            this.logger.LogWarning("Task {TaskId} not found", taskId);
            throw new KeyNotFoundException($"Task {taskId} not found.");
        }

        if (task.TodoList.OwnerId != userId && task.AssignedUserId != userId)
        {
            this.logger.LogWarning("User {UserId} not authorized to remove tags from task {TaskId}", userId, taskId);
            throw new UnauthorizedAccessException("You cannot remove tags from this task.");
        }

        var taskTag = task.TaskTags.FirstOrDefault(tt => tt.Tag.Name == tagName);
        if (taskTag != null)
        {
            _ = task.TaskTags.Remove(taskTag);
            _ = await this.context.SaveChangesAsync();
            this.logger.LogInformation("Successfully removed tag '{TagName}' from task {TaskId}", tagName, taskId);
        }
        else
        {
            this.logger.LogInformation("Tag '{TagName}' not found on task {TaskId}", tagName, taskId);
        }
    }

    // ---------------- Get Task With Tags ----------------
    public async Task<TaskWithTagsViewModel> GetTaskWithTagsAsync(int taskId, string userId)
    {
        this.logger.LogInformation("Getting task {TaskId} with tags for user {UserId}", taskId, userId);

        var task = await this.context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            this.logger.LogWarning("Task {TaskId} not found or access denied for user {UserId}", taskId, userId);
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

        this.logger.LogInformation("Retrieved task {TaskId} with {TagCount} tags", taskId, task.TaskTags.Count);
        return new TaskWithTagsViewModel
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            AssignedUserId = task.AssignedUserId,
            TodoListId = task.TodoListId,
            Tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList(),
        };
    }

    // ---------------- Get All Tags ----------------
    public async Task<List<string>> GetAllTagsAsync(string userId)
    {
        logger.LogInformation("Getting all tags for user {UserId}", userId);

        var tags = await this.context.TodoTaskTags
            .Include(tt => tt.TodoTask)
            .ThenInclude(t => t.TodoList)
            .Where(tt => tt.TodoTask.TodoList.OwnerId == userId || tt.TodoTask.AssignedUserId == userId)
            .Select(tt => tt.Tag.Name)
            .Distinct()
            .ToListAsync();

        logger.LogInformation("Retrieved {TagCount} distinct tags for user {UserId}", tags.Count, userId);
        return tags;
    }

    // ---------------- Get Tasks By Tag ----------------
    public async Task<IEnumerable<TodoTask>> GetTasksByTagAsync(string userId, string tagName)
    {
        logger.LogInformation("Getting tasks with tag '{TagName}' for user {UserId}", tagName, userId);

        var tasks = await this.context.TodoTaskTags
            .Include(tt => tt.TodoTask)
            .ThenInclude(t => t.TodoList)
            .Where(tt => tt.Tag.Name == tagName &&
                         (tt.TodoTask.TodoList.OwnerId == userId || tt.TodoTask.AssignedUserId == userId))
            .Select(tt => tt.TodoTask)
            .ToListAsync();

        logger.LogInformation("Retrieved {TaskCount} tasks with tag '{TagName}' for user {UserId}", tasks.Count, tagName, userId);
        return tasks.Select(t => new TodoTask
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            TodoListId = t.TodoListId,
            AssignedUserId = t.AssignedUserId,
        });
    }

    public async Task<List<string>> GetTagsForTaskAsync(int taskId, string userId)
    {
        logger.LogInformation("Getting tags for task {TaskId} for user {UserId}", taskId, userId);

        var task = await this.context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            logger.LogWarning("Task {TaskId} not found or access denied for user {UserId}", taskId, userId);
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

        var tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList();
        logger.LogInformation("Retrieved {TagCount} tags for task {TaskId}", tags.Count, taskId);
        return tags;
    }
}
