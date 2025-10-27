using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace TodoListApp.WebApi.Services;

public class PermissionService : IPermissionService
{
    private readonly TodoListDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(TodoListDbContext context, ILogger<PermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> GetUserRoleAsync(int todoListId, string userId)
    {
        // Check if user is the owner
        var todoList = await _context.TodoLists
            .FirstOrDefaultAsync(tl => tl.Id == todoListId);

        if (todoList?.OwnerId == userId)
        {
            return "Owner";
        }

        // Check if user has a role in TodoListUsers
        var todoListUser = await _context.Set<TodoListUser>()
            .FirstOrDefaultAsync(tlu => tlu.TodoListId == todoListId && tlu.UserId == userId);

        return todoListUser?.Role;
    }

    public async Task<bool> CanViewTodoListAsync(int todoListId, string userId)
    {
        var role = await GetUserRoleAsync(todoListId, userId);
        return role != null; // Owner, Editor, or Viewer can view
    }

    public async Task<bool> CanEditTodoListAsync(int todoListId, string userId)
    {
        var role = await GetUserRoleAsync(todoListId, userId);
        return role == "Owner" || role == "Editor";
    }

    public async Task<bool> CanDeleteTodoListAsync(int todoListId, string userId)
    {
        var role = await GetUserRoleAsync(todoListId, userId);
        return role == "Owner"; // Only owners can delete
    }

    public async Task<bool> CanManageTasksAsync(int todoListId, string userId)
    {
        var role = await GetUserRoleAsync(todoListId, userId);
        return role == "Owner" || role == "Editor";
    }

    public async Task<bool> CanViewTaskAsync(int taskId, string userId)
    {
        var task = await _context.TodoTasks
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
        return await CanViewTodoListAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanEditTaskAsync(int taskId, string userId)
    {
        var task = await _context.TodoTasks
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
        return await CanManageTasksAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanDeleteTaskAsync(int taskId, string userId)
    {
        var task = await _context.TodoTasks
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
        var task = await _context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Only owners and editors can manage tags
        return await CanManageTasksAsync(task.TodoListId, userId);
    }

    public async Task<bool> CanManageCommentsAsync(int taskId, string userId)
    {
        var task = await _context.TodoTasks
            .Include(t => t.TodoList)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        // Owners, editors, and assigned users can manage comments
        var role = await GetUserRoleAsync(task.TodoListId, userId);
        return role == "Owner" || role == "Editor" || task.AssignedUserId == userId;
    }
}
