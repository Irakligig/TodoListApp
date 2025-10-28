using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace TodoListApp.WebApi.Services;

public class PermissionService : IPermissionService
{
    private readonly TodoListDbContext context;
    private readonly ILogger<PermissionService> logger;

    public PermissionService(TodoListDbContext context, ILogger<PermissionService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<string?> GetUserRoleAsync(int todoListId, string userId)
    {
        // Check if user is the owner
        var todoList = await this.context.TodoLists
            .FirstOrDefaultAsync(tl => tl.Id == todoListId);

        if (todoList?.OwnerId == userId)
        {
            return "Owner";
        }

        // Check if user has a role in TodoListUsers
        var todoListUser = await this.context.Set<TodoListUser>()
            .FirstOrDefaultAsync(tlu => tlu.TodoListId == todoListId && tlu.UserId == userId);

        return todoListUser?.Role;
    }

    public async Task<bool> CanViewTodoListAsync(int todoListId, string userId)
    {
        var role = await this.GetUserRoleAsync(todoListId, userId);
        return role != null; // Owner, Editor, or Viewer can view
    }

    public async Task<bool> CanEditTodoListAsync(int todoListId, string userId)
    {
        var role = await this.GetUserRoleAsync(todoListId, userId);
        return role == "Owner" || role == "Editor";
    }

    public async Task<bool> CanDeleteTodoListAsync(int todoListId, string userId)
    {
        var role = await this.GetUserRoleAsync(todoListId, userId);
        return role == "Owner"; // Only owners can delete
    }

    public async Task<bool> CanManageTasksAsync(int todoListId, string userId)
    {
        var role = await this.GetUserRoleAsync(todoListId, userId);
        return role == "Owner" || role == "Editor";
    }

    public async Task<bool> CanViewTaskAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Check if user is assigned to the task
        if (task.AssignedUserId == userId)
        {
            return true;
        }

        // Check if user has access to the todo list
        return await this.CanViewTodoListAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanEditTaskAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Check if user is assigned to the task (can update status)
        if (task.AssignedUserId == userId)
        {
            return true;
        }

        // Check if user can manage tasks in the todo list
        return await this.CanManageTasksAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanDeleteTaskAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Only owners and editors can delete tasks
        return await CanManageTasksAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanManageTagsAsync(int taskId, string userId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Only owners and editors can manage tags
        return await this.CanManageTasksAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanManageCommentsAsync(int taskId, string userId)
    {
        try
        {
            var task = await this.context.TodoTasks
                .Include(t => t.TodoList)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return false;
            }

            var role = await GetUserRoleAsync(task.TodoListId, userId);

            if (string.IsNullOrEmpty(role))
            {
                return false; // User has no access to this list
            }

            // ALL users with access to the task can manage comments (including Viewers)
            // This allows Viewers to participate in discussions
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in CanManageCommentsAsync");
            return false;
            throw;
        }
    }
}
