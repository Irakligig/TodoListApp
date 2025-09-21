using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        try
        {
            var tasks = await taskService.GetAllTasksAsync(todoListId, userId);

            if (!tasks.Any())
            {
                return NotFound(new { message = "No tasks found." });
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

            return Ok(models);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // GET: api/todotask/task/{id}
    [HttpGet("task/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var task = await taskService.GetTaskByIdAsync(id, userId);
        if (task == null)
        {
            return NotFound(new { message = $"Task with Id {id} not found." });
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

        return Ok(model);
    }

    // POST: api/todotask
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoTaskModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var taskDto = new TodoTask
        {
            Name = model.Name,
            Description = model.Description,
            DueDate = model.DueDate,
            IsCompleted = model.IsCompleted,
            TodoListId = model.TodoListId
        };

        try
        {
            await taskService.AddTaskAsync(taskDto, userId);
            model.Id = taskDto.Id;
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // PUT: api/todotask/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoTaskModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var taskDto = new TodoTask
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
            DueDate = model.DueDate,
            IsCompleted = model.IsCompleted,
            TodoListId = model.TodoListId
        };

        try
        {
            await taskService.UpdateTaskAsync(taskDto, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // DELETE: api/todotask/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        try
        {
            await taskService.DeleteTaskAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("assigned")]
    public async Task<IActionResult> GetAllAssigned([FromQuery] string status, [FromQuery] string sortby)
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        try
        {
            await taskService.UpdateTaskStatusAsync(id, isCompleted, userId);
            return NoContent(); // 204 - success with no body
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

}
