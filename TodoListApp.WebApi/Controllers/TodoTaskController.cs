using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/todolists/{todoListId}/tasks")]
#pragma warning disable S6960 // Controllers should not have mixed responsibilities
    public class TodoTaskController : ControllerBase
    {
        private readonly ITodoTaskDatabaseService taskService;
        private readonly ITodoTaskTagDatabaseService tagService;

        public TodoTaskController(
            ITodoTaskDatabaseService taskService,
            ITodoTaskTagDatabaseService tagService)
        {
            this.taskService = taskService;
            this.tagService = tagService;
        }

        // GET: api/todolists/{todoListId}/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll(int todoListId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            var tasks = await this.taskService.GetAllTasksAsync(todoListId, userId);

            var models = tasks.Select(task => new TodoTaskModel
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                TodoListId = task.TodoListId,
                AssignedUserId = task.AssignedUserId,
                OwnerId = task.OwnerId,
            }).ToList();

            return this.Ok(models);
        }

        // GET: api/todolists/{todoListId}/tasks/{taskId}
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetById(int todoListId, int taskId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            var task = await this.taskService.GetTaskByIdAsync(taskId, userId);

            if (task == null || task.TodoListId != todoListId)
            {
                return this.NotFound(new { message = $"Task with Id {taskId} not found in List {todoListId}." });
            }

            var model = new TodoTaskModel
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                TodoListId = task.TodoListId,
                AssignedUserId = task.AssignedUserId,
                OwnerId = task.OwnerId
            };

            return this.Ok(model);
        }

        // POST: api/todolists/{todoListId}/tasks
        [HttpPost]
        public async Task<IActionResult> Create(int todoListId, [FromBody] TodoTaskModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            var taskDto = new TodoTask
            {
                Name = model.Name,
                Description = model.Description,
                DueDate = model.DueDate,
                IsCompleted = model.IsCompleted,
                TodoListId = todoListId,
                AssignedUserId = userId,
                OwnerId = userId,
            };

            try
            {
                await this.taskService.AddTaskAsync(taskDto, userId);
                model.Id = taskDto.Id;
                model.TodoListId = todoListId;
                model.AssignedUserId = taskDto.AssignedUserId;
                model.OwnerId = userId;

                return this.CreatedAtAction(
                    nameof(this.GetById),
                    new { todoListId, taskId = model.Id },
                    model);
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return this.Forbid(ex.Message);
            }
        }

        // PUT: api/todolists/{todoListId}/tasks/{taskId}
        [HttpPut("{taskId}")]
        public async Task<IActionResult> Update(int todoListId, int taskId, [FromBody] TodoTaskModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            var taskDto = new TodoTask
            {
                Id = taskId,
                Name = model.Name,
                Description = model.Description,
                DueDate = model.DueDate,
                IsCompleted = model.IsCompleted,
                TodoListId = todoListId
            };

            try
            {
                await this.taskService.UpdateTaskAsync(taskDto, userId);
                return this.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return this.Forbid();
            }
        }

        // DELETE: api/todolists/{todoListId}/tasks/{taskId}
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> Delete(int taskId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            try
            {
                await this.taskService.DeleteTaskAsync(taskId, userId);
                return this.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return this.Forbid();
            }
        }

        // GET: api/tasks/assigned
        [HttpGet("~/api/tasks/assigned")]
        public async Task<IActionResult> GetAllAssigned([FromQuery] string? status = null, [FromQuery] string? sortBy = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            try
            {
                var tasks = await this.taskService.GetAssignedTasksAsync(userId, status, sortBy);

                var models = tasks.Select(t => new TodoTaskModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    IsCompleted = t.IsCompleted,
                    TodoListId = t.TodoListId,
                    AssignedUserId = t.AssignedUserId,
                    OwnerId = t.OwnerId
                });

                return this.Ok(models);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(new { message = ex.Message });
            }
        }

        // PATCH: api/tasks/assigned/{taskId}/status?isCompleted=true
        [HttpPatch("~/api/tasks/assigned/{taskId}/status")]
        public async Task<IActionResult> ChangeTaskStatus(int taskId, [FromQuery] bool isCompleted)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            try
            {
                await this.taskService.UpdateTaskStatusAsync(taskId, isCompleted, userId);
                return this.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return this.Forbid();
            }
        }

        [HttpPatch("~/api/tasks/assigned/{taskId}/assign")]
        public async Task<IActionResult> ReassignTask(int taskId, [FromBody] ReassignTaskDto dto)
        {
            var currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            if (dto == null)
            {
                return this.BadRequest("DTO is null");
            }

            if (string.IsNullOrWhiteSpace(dto.NewUserId))
            {
                return this.BadRequest("NewUserId is missing");
            }

            try
            {
                await this.taskService.ReassignTaskAsync(taskId, currentUserId, dto.NewUserId);
                return this.Ok(new { message = "Task reassigned successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return this.Forbid();
            }
        }

        [HttpGet("~/api/tasks/search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? query = null,
            [FromQuery] bool? status = null,
            [FromQuery] DateTime? dueBefore = null,
            [FromQuery] string? assignedUserId = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            try
            {
                var results = await this.taskService.SearchTasksAsync(userId, query, status, dueBefore, assignedUserId);
                return this.Ok(results);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return this.BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{taskId}/details")]
        public async Task<IActionResult> GetTaskDetails(int taskId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            try
            {
                var taskWithTags = await this.tagService.GetTaskWithTagsAsync(taskId, userId);
                return this.Ok(taskWithTags);
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { message = ex.Message });
            }
        }
    }
}
