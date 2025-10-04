using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Data;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

[Authorize] // require authentication
[ApiController]
[Route("api/[controller]")]
public class TodoListController : ControllerBase
{
    private readonly ITodoListDatabaseService todoListService;

    public TodoListController(ITodoListDatabaseService todoListService)
    {
        this.todoListService = todoListService;
    }

    // GET: api/todolist
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        Console.WriteLine($"Current ownerId: {userId}");

        var lists = await this.todoListService.GetAllTodoListsAsync(userId);

        //if (!lists.Any()) // check if empty
        //{
        //    return this.NotFound(new { message = "No todo lists found." });
        //}

        // Map DTO to model for returning
        var models = lists.Select(t => new TodoListModel
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            OwnerId = t.OwnerId,
        });

        return this.Ok(models);
    }

    // POST: api/todolist
    [HttpPost]
    public async Task<IActionResult> Create(TodoListModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var todoListDto = new TodoList
        {
            Name = model.Name,
            Description = model.Description,
            OwnerId = model.OwnerId,
        };

        try
        {
            await this.todoListService.AddTodoListAsync(todoListDto, userId);
            model.Id = todoListDto.Id; // get generated Id
            return this.Ok(new { id = model.Id, message = "Todo list created successfully" });
        }
        catch (ArgumentException ex)
        {
            return this.BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/todolist/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoListModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var todoListDto = new TodoList
        {
            Id = id,
            Name = model.Name,
            Description = model.Description,
        };

        try
        {
            await this.todoListService.UpdateTodoListAsync(todoListDto, userId);

            // Return updated model as JSON
            return this.Ok(new TodoListModel
            {
                Id = todoListDto.Id,
                Name = todoListDto.Name,
                Description = todoListDto.Description,
                OwnerId = todoListDto.OwnerId,
            });
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

    // DELETE: api/todolist/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        try
        {
            await this.todoListService.DeleteTodoListAsync(id, userId);
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
}
