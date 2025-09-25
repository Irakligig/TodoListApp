using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
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
        // Ensure the list belongs to the user
        var list = await this.context.TodoLists.FindAsync(todoListId);
        if (list == null || list.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have access to this todo list.");
        }

        return await this.context.TodoTasks
            .Where(t => t.TodoListId == todoListId)
            .Select(t => new TodoTask
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                TodoListId = t.TodoListId,
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

        if (task.TodoList.OwnerId != ownerId)
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
            DueDate = task.DueDate ?? DateTime.MinValue,
            IsCompleted = task.IsCompleted,
            TodoListId = task.TodoListId,
            OwnerId = ownerId,
        };

        _ = await this.context.TodoTasks.AddAsync(entity);
        _ = await this.context.SaveChangesAsync();

        task.Id = entity.Id; // return generated ID
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

        _ = await this.context.SaveChangesAsync();
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

        _ = this.context.TodoTasks.Remove(entity);
        _ = await this.context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoTask>> GetAssignedTasksAsync(string userId, string? status = null, string? sortby = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.");
        }

        var query = this.context.TodoTasks.AsQueryable();

        query = query.Where(t => t.AssignedUserId == userId);

        _ = status?.ToLower() switch
        {
            "active" => query.Where(t => !t.IsCompleted),
            "completed" => query.Where(t => t.IsCompleted),
            "overdue" => query.Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted),
            _ => query
        };

        _ = sortby?.ToLower() switch
        {
            "duedate" => query.OrderBy(t => t.DueDate),
            "name" => query.OrderBy(t => t.Name),
            _ => query.OrderBy(t => t.Id)
        };

        return await query.Select(t => new TodoTask
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            TodoListId = t.TodoListId,
            AssignedUserId = t.AssignedUserId,
        }).ToListAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, bool isCompleted, string userId)
    {
        // Find task assigned to the current user
        var task = await this.context.TodoTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedUserId == userId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task with Id {taskId} not found or not assigned to user.");
        }

        // Update status
        task.IsCompleted = isCompleted;

        _ = await this.context.SaveChangesAsync();
    }
}
