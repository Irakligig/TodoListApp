using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public interface ITodoListShareService
{
    Task ShareTodoListAsync(int todoListId, string targetUserId, string role, string ownerId);

    Task UpdateShareAsync(int todoListId, string targetUserId, string newRole, string ownerId);

    Task RemoveShareAsync(int todoListId, string targetUserId, string ownerId);

    Task<IEnumerable<TodoListUser>> GetSharedUsersAsync(int todoListId, string userId);

    Task<IEnumerable<SharedTodoListDto>> GetSharedWithMeAsync(string userId);
}
