namespace TodoListApp.WebApi.Services
{
    public interface IPermissionService
    {
        Task<string?> GetUserRoleAsync(int todoListId, string userId);

        Task<bool> CanViewTodoListAsync(int todoListId, string userId);

        Task<bool> CanEditTodoListAsync(int todoListId, string userId);

        Task<bool> CanDeleteTodoListAsync(int todoListId, string userId);

        Task<bool> CanManageTasksAsync(int todoListId, string userId);

        Task<bool> CanViewTaskAsync(int taskId, string userId);

        Task<bool> CanEditTaskAsync(int taskId, string userId);

        Task<bool> CanDeleteTaskAsync(int taskId, string userId);

        Task<bool> CanManageTagsAsync(int taskId, string userId);

        Task<bool> CanManageCommentsAsync(int taskId, string userId);
    }
}
