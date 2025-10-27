using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database;
using TodoListApp.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace TodoListApp.WebApi.Services;

public class TodoListShareService : ITodoListShareService
{
    private readonly TodoListDbContext context;
    private readonly ILogger<TodoListShareService> logger;
    private readonly IPermissionService permissionService;

    public TodoListShareService(
        TodoListDbContext context,
        ILogger<TodoListShareService> logger,
        IPermissionService permissionService)
    {
        this.context = context;
        this.logger = logger;
        this.permissionService = permissionService;
    }

    public async Task ShareTodoListAsync(int todoListId, string targetUserId, string role, string ownerId)
    {
        this.logger.LogInformation(
            "Sharing todo list {TodoListId} with user {TargetUserId} as {Role}",
            todoListId, targetUserId, role);

        // Validate role
        if (role != "Viewer" && role != "Editor")
        {
            throw new ArgumentException("Role must be either 'Viewer' or 'Editor'");
        }

        // Check if owner has permission to share
        var ownerRole = await this.permissionService.GetUserRoleAsync(todoListId, ownerId);
        if (ownerRole != "Owner")
        {
            throw new UnauthorizedAccessException("Only the owner can share the todo list");
        }

        // Check if todo list exists
        var todoList = await this.context.TodoLists.FindAsync(todoListId);
        if (todoList == null)
        {
            throw new KeyNotFoundException($"Todo list with ID {todoListId} not found");
        }

        // Prevent sharing with yourself
        if (targetUserId == ownerId)
        {
            throw new InvalidOperationException("Cannot share todo list with yourself");
        }

        // Check if already shared
        var existingShare = await this.context.Set<TodoListUser>()
            .FirstOrDefaultAsync(tlu => tlu.TodoListId == todoListId && tlu.UserId == targetUserId);

        if (existingShare != null)
        {
            throw new InvalidOperationException("Todo list is already shared with this user");
        }

        // Create share record
        var todoListUser = new TodoListUser
        {
            TodoListId = todoListId,
            UserId = targetUserId,
            Role = role,
        };

        _ = await this.context.Set<TodoListUser>().AddAsync(todoListUser);
        _ = await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully shared todo list {TodoListId} with user {TargetUserId} as {Role}",
            todoListId, targetUserId, role);
    }

    public async Task UpdateShareAsync(int todoListId, string targetUserId, string newRole, string ownerId)
    {
        this.logger.LogInformation("Updating share role for todo list {TodoListId}, user {TargetUserId} to {NewRole}",
            todoListId, targetUserId, newRole);

        // Validate role
        if (newRole != "Viewer" && newRole != "Editor")
        {
            throw new ArgumentException("Role must be either 'Viewer' or 'Editor'");
        }

        // Check if owner has permission
        var ownerRole = await this.permissionService.GetUserRoleAsync(todoListId, ownerId);
        if (ownerRole != "Owner")
        {
            throw new UnauthorizedAccessException("Only the owner can update sharing permissions");
        }

        var share = await this.context.Set<TodoListUser>()
            .FirstOrDefaultAsync(tlu => tlu.TodoListId == todoListId && tlu.UserId == targetUserId);

        if (share == null)
        {
            throw new KeyNotFoundException("Share record not found");
        }

        share.Role = newRole;
        _ = await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully updated share role for todo list {TodoListId}, user {TargetUserId} to {NewRole}",
            todoListId, targetUserId, newRole);
    }

    public async Task RemoveShareAsync(int todoListId, string targetUserId, string ownerId)
    {
        this.logger.LogInformation("Removing share for todo list {TodoListId} from user {TargetUserId}",
            todoListId, targetUserId);

        // Check if owner has permission
        var ownerRole = await this.permissionService.GetUserRoleAsync(todoListId, ownerId);
        if (ownerRole != "Owner")
        {
            throw new UnauthorizedAccessException("Only the owner can remove sharing");
        }

        var share = await this.context.Set<TodoListUser>()
            .FirstOrDefaultAsync(tlu => tlu.TodoListId == todoListId && tlu.UserId == targetUserId);

        if (share == null)
        {
            throw new KeyNotFoundException("Share record not found");
        }

        _ = this.context.Set<TodoListUser>().Remove(share);
        _ = await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully removed share for todo list {TodoListId} from user {TargetUserId}",
            todoListId, targetUserId);
    }

    public async Task<IEnumerable<TodoListUser>> GetSharedUsersAsync(int todoListId, string userId)
    {
        this.logger.LogInformation("Getting shared users for todo list {TodoListId}", todoListId);

        // Check if user has access to the todo list
        if (!await this.permissionService.CanViewTodoListAsync(todoListId, userId))
        {
            throw new UnauthorizedAccessException("You don't have access to this todo list");
        }

        var sharedUsers = await this.context.Set<TodoListUser>()
            .Where(tlu => tlu.TodoListId == todoListId)
            .ToListAsync();

        return sharedUsers;
    }

    public async Task<IEnumerable<SharedTodoListDto>> GetSharedWithMeAsync(string userId)
    {
        this.logger.LogInformation("Getting todo lists shared with user {UserId}", userId);

        var sharedLists = await this.context.Set<TodoListUser>()
       .Where(tlu => tlu.UserId == userId) // User is in TodoListUsers
       .Join(
            this.context.TodoLists,
           tlu => tlu.TodoListId,
           tl => tl.Id,
           (tlu, tl) => new { TodoListUser = tlu, TodoList = tl })
       .Where(x => x.TodoList.OwnerId != userId) // EXCLUDE lists where user is the owner
       .Select(x => new SharedTodoListDto
       {
           TodoListId = x.TodoList.Id,
           Name = x.TodoList.Name,
           Description = x.TodoList.Description,
           OwnerName = x.TodoList.OwnerId, // This should be the actual owner's username
           Role = x.TodoListUser.Role,
           SharedAt = DateTime.UtcNow // You might want to add CreatedAt to TodoListUser
       })
       .ToListAsync();
        return sharedLists;
    }
}
