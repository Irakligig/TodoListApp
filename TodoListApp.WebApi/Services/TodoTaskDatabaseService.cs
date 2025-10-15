using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public class TodoTaskDatabaseService : ITodoTaskDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ILogger<TodoTaskDatabaseService> logger;

    public TodoTaskDatabaseService(TodoListDbContext context, ILogger<TodoTaskDatabaseService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<IEnumerable<TodoTask>> GetAllTasksAsync(int todoListId, string ownerId)
    {
        logger.LogInformation("Getting all tasks for todo list {TodoListId} for owner {OwnerId}", todoListId, ownerId);

        var list = await this.context.TodoLists.FindAsync(todoListId);

        if (list == null)
        {
            logger.LogWarning("Todo list with Id {TodoListId} not found", todoListId);
            throw new KeyNotFoundException($"Todo list with Id {todoListId} not found.");
        }

        if (!string.Equals(list.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("User {OwnerId} does not have access to todo list {TodoListId}", ownerId, todoListId);
            throw new UnauthorizedAccessException("You do not have access to this todo list.");
        }

        var tasks = await this.context.TodoTasks
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(t => t.TodoListId == todoListId)
            .Select(t => new TodoTask()
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                TodoListId = t.TodoListId,
                AssignedUserId = t.AssignedUserId,
            })
            .ToListAsync();

        logger.LogInformation("Retrieved {TaskCount} tasks for todo list {TodoListId}", tasks.Count, todoListId);
        return tasks;
    }

    public async Task<TodoTask?> GetTaskByIdAsync(int taskId, string ownerId)
    {
        logger.LogInformation("Getting task {TaskId} for user {OwnerId}", taskId, ownerId);

        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            logger.LogInformation("Task {TaskId} not found", taskId);
            return null;
        }

        if (task.TodoList.OwnerId != ownerId && task.AssignedUserId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have access to task {TaskId}", ownerId, taskId);
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

        logger.LogInformation("Successfully retrieved task {TaskId}", taskId);
        return new TodoTask
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            TodoListId = task.TodoListId,
            AssignedUserId = task.AssignedUserId
        };
    }

    public async Task AddTaskAsync(TodoTask task, string ownerId)
    {
        logger.LogInformation("Adding new task to list {TodoListId} for owner {OwnerId}", task.TodoListId, ownerId);

        var list = await this.context.TodoLists.FindAsync(task.TodoListId);

        if (list == null)
        {
            logger.LogWarning("Todo list with Id {TodoListId} not found", task.TodoListId);
            throw new KeyNotFoundException($"Todo list with Id {task.TodoListId} not found.");
        }

        if (list.OwnerId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have access to add tasks to list {TodoListId}", ownerId, task.TodoListId);
            throw new UnauthorizedAccessException("You do not have access to add tasks to this list.");
        }

        var entity = new TodoTaskEntity
        {
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            TodoListId = task.TodoListId,
            OwnerId = ownerId,
            AssignedUserId = task.AssignedUserId
        };

        await this.context.TodoTasks.AddAsync(entity);
        await this.context.SaveChangesAsync();

        task.Id = entity.Id;
        logger.LogInformation("Successfully added task {TaskId} to list {TodoListId}", task.Id, task.TodoListId);
    }

    public async Task UpdateTaskAsync(TodoTask task, string ownerId)
    {
        logger.LogInformation("Updating task {TaskId} for owner {OwnerId}", task.Id, ownerId);

        var entity = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        if (entity == null)
        {
            logger.LogWarning("Task with Id {TaskId} not found", task.Id);
            throw new KeyNotFoundException($"Task with Id {task.Id} not found.");
        }

        if (entity.TodoList.OwnerId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have access to update task {TaskId}", ownerId, task.Id);
            throw new UnauthorizedAccessException("You do not have access to update this task.");
        }

        entity.Name = task.Name;
        entity.Description = task.Description;
        entity.DueDate = task.DueDate ?? entity.DueDate;
        entity.IsCompleted = task.IsCompleted;
        entity.AssignedUserId = task.AssignedUserId;

        await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully updated task {TaskId}", task.Id);
    }

    public async Task DeleteTaskAsync(int taskId, string ownerId)
    {
        logger.LogInformation("Deleting task {TaskId} for owner {OwnerId}", taskId, ownerId);

        var entity = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (entity == null)
        {
            logger.LogWarning("Task with Id {TaskId} not found", taskId);
            throw new KeyNotFoundException($"Task with Id {taskId} not found.");
        }

        if (entity.TodoList.OwnerId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have access to delete task {TaskId}", ownerId, taskId);
            throw new UnauthorizedAccessException("You do not have access to delete this task.");
        }

        this.context.TodoTasks.Remove(entity);
        await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully deleted task {TaskId}", taskId);
    }

    public async Task<IEnumerable<TodoTask>> GetAssignedTasksAsync(string userId, string? status = null, string? sortby = null)
    {
        logger.LogInformation("Getting assigned tasks for user {UserId} with status '{Status}'", userId, status ?? "all");

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogError("User ID cannot be null or empty");
            throw new ArgumentException("User ID cannot be null or empty.");
        }

        var query = this.context.TodoTasks.AsQueryable();
        query = query.Where(t => t.AssignedUserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            switch (status.ToLower())
            {
                case "pending":
                    query = query.Where(t => !t.IsCompleted);
                    break;
                case "completed":
                    query = query.Where(t => t.IsCompleted);
                    break;
                case "overdue":
                    query = query.Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted);
                    break;
                default:
                    logger.LogWarning("Unknown status filter: {Status}", status);
                    break;
            }
        }

        // Optional sorting (not fully implemented)
        // TODO: implement sortBy if needed

        var tasks = await query.Select(t => new TodoTask
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            TodoListId = t.TodoListId,
            AssignedUserId = t.AssignedUserId
        }).ToListAsync();

        logger.LogInformation("Retrieved {TaskCount} assigned tasks for user {UserId}", tasks.Count, userId);
        return tasks;
    }

    public async Task UpdateTaskStatusAsync(int taskId, bool isCompleted, string userId)
    {
        logger.LogInformation("Updating task {TaskId} status to {Status} for user {UserId}", taskId, isCompleted ? "completed" : "pending", userId);

        var task = await this.context.TodoTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

        if (task == null)
        {
            logger.LogWarning("Task with Id {TaskId} not found or not assigned to user {UserId}", taskId, userId);
            throw new KeyNotFoundException($"Task with Id {taskId} not found or not assigned to user.");
        }

        task.IsCompleted = isCompleted;
        await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully updated task {TaskId} status to {Status}", taskId, isCompleted ? "completed" : "pending");
    }

    public async Task<TodoTask?> GetTaskByIdForAssignedUserAsync(int taskId, string userId)
    {
        logger.LogInformation("Getting task {TaskId} for assigned user {UserId}", taskId, userId);

        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

        if (task == null)
        {
            logger.LogInformation("Task {TaskId} not found for assigned user {UserId}", taskId, userId);
            return null;
        }

        logger.LogInformation("Successfully retrieved task {TaskId} for assigned user {UserId}", taskId, userId);
        return new TodoTask
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            TodoListId = task.TodoListId,
            AssignedUserId = task.AssignedUserId
        };
    }

    public async Task ReassignTaskAsync(int taskId, string currentUserId, string newUserId)
    {
        logger.LogInformation("Reassigning task {TaskId} from user {CurrentUserId} to user {NewUserId}", taskId, currentUserId, newUserId);

        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            logger.LogWarning("Task {TaskId} not found", taskId);
            throw new KeyNotFoundException($"Task {taskId} not found.");
        }

        if (task.AssignedUserId != currentUserId)
        {
            logger.LogWarning("User {CurrentUserId} is not allowed to reassign task {TaskId}", currentUserId, taskId);
            throw new UnauthorizedAccessException("You are not allowed to reassign this task.");
        }

        // With Identity, we assume newUserId exists. Optional: validate via UserManager

        task.AssignedUserId = newUserId;
        await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully reassigned task {TaskId} to user {NewUserId}", taskId, newUserId);
    }

    public async Task<IEnumerable<TodoTask>> SearchTasksAsync(string userId, string? query, bool? status, DateTime? dueBefore, string? assignedUserId)
    {
        logger.LogInformation("Searching tasks for user {UserId} with query '{Query}', status: {Status}, dueBefore: {DueBefore}, assignedUserId: {AssignedUserId}",
            userId, query, status, dueBefore, assignedUserId);

        if (string.IsNullOrWhiteSpace(userId))
        {
            logger.LogError("User ID is required for task search");
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        var tasks = context.TodoTasks.Include(t => t.TodoList)
            .Where(t => t.TodoList.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(query))
        {
            tasks = tasks.Where(t => t.Name.Contains(query) || (t.Description ?? "").Contains(query));
        }

        if (status.HasValue)
        {
            tasks = tasks.Where(t => t.IsCompleted == status.Value);
        }

        if (dueBefore.HasValue)
        {
            tasks = tasks.Where(t => t.DueDate <= dueBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(assignedUserId))
        {
            tasks = tasks.Where(t => t.AssignedUserId == assignedUserId);
        }

        var result = await tasks.AsNoTracking()
            .Select(t => new TodoTask
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                TodoListId = t.TodoListId,
                AssignedUserId = t.AssignedUserId
            }).ToListAsync();

        logger.LogInformation("Search returned {TaskCount} tasks for user {UserId}", result.Count, userId);
        return result;
    }
}
