using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public class TodoTaskDatabaseService : ITodoTaskDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly IUsersDatabaseService usersService; // inject user service

    public TodoTaskDatabaseService(
        TodoListDbContext context,
        IUsersDatabaseService usersService) // constructor injection
    {
        this.context = context;
        this.usersService = usersService;
    }

    public async Task<IEnumerable<TodoTask>> GetAllTasksAsync(int todoListId, string ownerId)
    {
        // Ensure the list belongs to the user
        var list = await this.context.TodoLists.FindAsync(todoListId);

        // If the list is NULL, throw KeyNotFoundException (maps to 404 in controller)
        if (list == null)
        {
            throw new KeyNotFoundException($"Todo list with Id {todoListId} not found.");
        }

        // If the list exists but owner doesn't match (using case-insensitive check for robustness)
        if (!string.Equals(list.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You do not have access to this todo list.");
        }
        return await this.context.TodoTasks
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(t => t.TodoListId == todoListId)
            .Where(t => t.AssignedUserId == ownerId)
            .Select(t => new TodoTask()
            {
                // Data mapping (projection) must be complete to avoid ID=0
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
            AssignedUserId = task.AssignedUserId,
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

    public async Task<IEnumerable<TodoTask>> GetAssignedTasksAsync(string userId, string? status = null, string? sortby = null) // Changed to sortBy
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.");
        }

        // Start with all tasks assigned to the user
        var query = this.context.TodoTasks.AsQueryable();
        query = query.Where(t => t.AssignedUserId == userId);

        // Apply status filter
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

        // Apply sorting - use sortBy instead of sortby
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "duedate" => query.OrderBy(t => t.DueDate.HasValue).ThenBy(t => t.DueDate),
                "duedate_desc" => query.OrderByDescending(t => t.DueDate.HasValue).ThenByDescending(t => t.DueDate),
                "name" or "title" => query.OrderBy(t => t.Name),
                "name_desc" or "title_desc" => query.OrderByDescending(t => t.Name),
                "status" => query.OrderBy(t => t.IsCompleted), // Sort by completion status
                "status_desc" => query.OrderByDescending(t => t.IsCompleted),
                _ => query.OrderBy(t => t.Id)
            };
        }
        else
        {
            query = query.OrderBy(t => t.Id);
        }

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

    public async Task<TodoTask?> GetTaskByIdForAssignedUserAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
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
        // 1. Load task including its TodoList
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task {taskId} not found.");
        }

        // 2. Ensure the current user is the one currently assigned
        if (task.AssignedUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to reassign this task.");
        }

        // 3. Validate new user exists (if users are in separate DB, call UsersService)
        var newUser = await this.usersService.GetByIdAsync(newUserId);
        if (newUser == null)
        {
            throw new KeyNotFoundException($"User {newUserId} not found.");
        }

        // 4. Update assignment
        task.AssignedUserId = newUserId;
        await this.context.SaveChangesAsync();
    }


}
