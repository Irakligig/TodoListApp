using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

[Authorize] // Require authentication
[ApiController]
[Route("api/[controller]")]
public class TodoTaskController : ControllerBase
{
    private readonly ITodoTaskDatabaseService taskService;

    public TodoTaskController(ITodoTaskDatabaseService taskService)
    {
        this.taskService = taskService;
    }

    // GET: api/todotask/{todoListId}
    [HttpGet("{todoListId}")]
    public async Task<IActionResult> GetAll(int todoListId)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        try
        {
            var tasks = await this.taskService.GetAllTasksAsync(todoListId, userId);

            if (!tasks.Any())
            {
                return this.NotFound(new { message = "No tasks found." });
            }

            var models = tasks.Select(t => new TodoTaskModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                TodoListId = t.TodoListId,
            });

            return this.Ok(models);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Forbid(ex.Message);
        }
    }

    // GET: api/todotask/task/{id}
    [HttpGet("task/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var task = await this.taskService.GetTaskByIdAsync(id, userId);
        if (task == null)
        {
            return this.NotFound(new { message = $"Task with Id {id} not found." });
        }

        var model = new TodoTaskModel
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            TodoListId = task.TodoListId,
        };

        return this.Ok(model);
    }

    // POST: api/todotask
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoTaskModel model)
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
            TodoListId = model.TodoListId,
        };

        try
        {
            await this.taskService.AddTaskAsync(taskDto, userId);
            model.Id = taskDto.Id;
            return this.CreatedAtAction(nameof(this.GetById), new { id = model.Id }, model);
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

    // PUT: api/todotask/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoTaskModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var taskDto = new TodoTask
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            DueDate = model.DueDate,
            IsCompleted = model.IsCompleted,
            TodoListId = model.TodoListId,
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
        catch (UnauthorizedAccessException ex)
        {
            return this.Forbid(ex.Message);
        }
    }

    // DELETE: api/todotask/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        try
        {
            await this.taskService.DeleteTaskAsync(id, userId);
            return this.NoContent();
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

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAllAssigned([FromQuery] string? status = null, [FromQuery] string? sortby = null)
    {
        var user = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        try
        {
            var tasks = await this.taskService.GetAssignedTasksAsync(user, status, sortby);
            var models = tasks.Select(t => new TodoTaskModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                TodoListId = t.TodoListId,
                AssignedUserId = user,
            });
            return this.Ok(models);
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("assigned/{id}/status")]
    public async Task<IActionResult> ChangeTaskStatus(int id, [FromQuery] bool isCompleted)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        try
        {
            await this.taskService.UpdateTaskStatusAsync(id, isCompleted, userId);
            return this.NoContent(); // 204 - success with no body
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
}
