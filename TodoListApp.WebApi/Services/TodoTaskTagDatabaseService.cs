using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Services;

public class TodoTaskTagDatabaseService : ITodoTaskTagDatabaseService
{
    private readonly TodoListDbContext context;

    public TodoTaskTagDatabaseService(TodoListDbContext context)
    {
        this.context = context;
    }

    // ---------------- Add Tag ----------------
    public async Task AddTagToTaskAsync(int taskId, string tagName, string userId)
    {
        var task = await context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

        var normalizedTagName = tagName.ToLower();
        var tag = await context.TodoTags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == normalizedTagName);

        if (tag == null)
        {
            tag = new TodoTagEntity { Name = tagName };
            await context.TodoTags.AddAsync(tag);
            await context.SaveChangesAsync();
        }

        if (!task.TaskTags.Any(tt => tt.TagId == tag.Id))
        {
            task.TaskTags.Add(new TodoTaskTagEntity
            {
                TodoTaskId = taskId,
                TagId = tag.Id
            });
            await context.SaveChangesAsync();
        }
    }

    // ---------------- Remove Tag ----------------
    public async Task RemoveTagFromTaskAsync(int taskId, string tagName, string userId)
    {
        var task = await context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found.");
        }

        if (task.TodoList.OwnerId != userId && task.AssignedUserId != userId)
        {
            throw new UnauthorizedAccessException("You cannot remove tags from this task.");
        }

        var taskTag = task.TaskTags.FirstOrDefault(tt => tt.Tag.Name == tagName);
        if (taskTag != null)
        {
            task.TaskTags.Remove(taskTag);
            await context.SaveChangesAsync();
        }
    }

    // ---------------- Get Task With Tags ----------------
    public async Task<TaskWithTagsViewModel> GetTaskWithTagsAsync(int taskId, string userId)
    {
        var task = await context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

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
        var tags = await context.TodoTaskTags
            .Include(tt => tt.TodoTask)
            .ThenInclude(t => t.TodoList)
            .Where(tt => tt.TodoTask.TodoList.OwnerId == userId || tt.TodoTask.AssignedUserId == userId)
            .Select(tt => tt.Tag.Name)
            .Distinct()
            .ToListAsync();

        return tags;
    }

    // ---------------- Get Tasks By Tag ----------------
    public async Task<IEnumerable<TodoTask>> GetTasksByTagAsync(string userId, string tagName)
    {
        var tasks = await context.TodoTaskTags
            .Include(tt => tt.TodoTask)
            .ThenInclude(t => t.TodoList)
            .Where(tt => tt.Tag.Name == tagName &&
                         (tt.TodoTask.TodoList.OwnerId == userId || tt.TodoTask.AssignedUserId == userId))
            .Select(tt => tt.TodoTask)
            .ToListAsync();

        return tasks.Select(t => new TodoTask
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            TodoListId = t.TodoListId,
            AssignedUserId = t.AssignedUserId
        });
    }

    public async Task<List<string>> GetTagsForTaskAsync(int taskId, string userId)
    {
        var task = await context.TodoTasks
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                                      (t.TodoList.OwnerId == userId || t.AssignedUserId == userId));

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found or access denied.");
        }

        return task.TaskTags.Select(tt => tt.Tag.Name).ToList();
    }
}
