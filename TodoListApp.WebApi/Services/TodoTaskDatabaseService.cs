using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public class TodoTaskDatabaseService : ITodoTaskDatabaseService
{
    private readonly TodoListDbContext context;

    public TodoTaskDatabaseService(TodoListDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<TodoTask>> GetAllTasksAsync(int todoListId, string ownerId)
    {
        var list = await this.context.TodoLists.FindAsync(todoListId);

        if (list == null)
        {
            throw new KeyNotFoundException($"Todo list with Id {todoListId} not found.");
        }

        if (!string.Equals(list.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You do not have access to this todo list.");
        }

        return await this.context.TodoTasks
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
    }

    public async Task<TodoTask?> GetTaskByIdAsync(int taskId, string ownerId)
    {
        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return null;
        }

        if (task.TodoList.OwnerId != ownerId && task.AssignedUserId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have access to this task.");
        }

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
        var list = await this.context.TodoLists.FindAsync(task.TodoListId);

        if (list == null)
        {
            throw new KeyNotFoundException($"Todo list with Id {task.TodoListId} not found.");
        }

        if (list.OwnerId != ownerId)
        {
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
    }

    public async Task UpdateTaskAsync(TodoTask task, string ownerId)
    {
        var entity = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == task.Id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Task with Id {task.Id} not found.");
        }

        if (entity.TodoList.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have access to update this task.");
        }

        entity.Name = task.Name;
        entity.Description = task.Description;
        entity.DueDate = task.DueDate ?? entity.DueDate;
        entity.IsCompleted = task.IsCompleted;
        entity.AssignedUserId = task.AssignedUserId;

        await this.context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int taskId, string ownerId)
    {
        var entity = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Task with Id {taskId} not found.");
        }

        if (entity.TodoList.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have access to delete this task.");
        }

        this.context.TodoTasks.Remove(entity);
        await this.context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoTask>> GetAssignedTasksAsync(string userId, string? status = null, string? sortby = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
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
                    break;
            }
        }

        // Optional sorting (not fully implemented)
        // TODO: implement sortBy if needed

        return await query.Select(t => new TodoTask
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            TodoListId = t.TodoListId,
            AssignedUserId = t.AssignedUserId
        }).ToListAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, bool isCompleted, string userId)
    {
        var task = await this.context.TodoTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with Id {taskId} not found or not assigned to user.");
        }

        task.IsCompleted = isCompleted;
        await this.context.SaveChangesAsync();
    }

    public async Task<TodoTask?> GetTaskByIdForAssignedUserAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

        if (task == null)
        {
            return null;
        }

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
        var task = await this.context.TodoTasks.Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found.");
        }

        if (task.AssignedUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to reassign this task.");
        }

        // With Identity, we assume newUserId exists. Optional: validate via UserManager

        task.AssignedUserId = newUserId;
        await this.context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoTask>> SearchTasksAsync(string userId, string? query, bool? status, DateTime? dueBefore, string? assignedUserId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
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

        return await tasks.AsNoTracking()
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
    }
}
