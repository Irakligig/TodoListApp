using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public interface ITodoTaskWebApiService
    {
        Task<IEnumerable<TodoTaskModel>> GetTasksAsync(int todoListId);
        Task<TodoTaskModel?> GetByIdAsync(int todoListId, int taskId);
        Task<TodoTaskModel?> CreateAsync(int todoListId, TodoTaskModel model);
        Task UpdateAsync(int todoListId, int taskId, TodoTaskModel model);
        Task DeleteAsync(int todoListId, int taskId);

        // Epic 3: Assigned / filtered view
        Task<IEnumerable<TodoTaskModel>> GetAssignedAsync(string? status = null, string? sortBy = null);
        Task UpdateStatusAsync(int taskId, bool status);
    }

}
