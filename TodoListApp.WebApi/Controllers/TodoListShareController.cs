using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Services;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoListShareController : ControllerBase
{
    private readonly ITodoListShareService shareService;
    private readonly ILogger<TodoListShareController> logger;

    public TodoListShareController(
        ITodoListShareService shareService,
        ILogger<TodoListShareController> logger)
    {
        this.shareService = shareService;
        this.logger = logger;
    }

    // Share a specific todo list with a user
    [HttpPost("todolists/{todoListId}/share")]
    public async Task<IActionResult> ShareTodoList(int todoListId, [FromBody] ShareRequest request)
    {
        try
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            await this.shareService.ShareTodoListAsync(todoListId, request.TargetUserId, request.Role, userId);
            return this.Ok(new { message = "Todo list shared successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error sharing todo list {TodoListId}", todoListId);
            throw;
        }
    }

    // Update sharing role for a user
    [HttpPut("todolists/{todoListId}/share/{targetUserId}")]
    public async Task<IActionResult> UpdateShare(int todoListId, string targetUserId, [FromBody] UpdateShareRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            await this.shareService.UpdateShareAsync(todoListId, targetUserId, request.NewRole, userId);
            return this.Ok(new { message = "Share updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating share for todo list {TodoListId}", todoListId);
            throw;
        }
    }

    // Remove sharing for a user
    [HttpDelete("todolists/{todoListId}/share/{targetUserId}")]
    public async Task<IActionResult> RemoveShare(int todoListId, string targetUserId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            await this.shareService.RemoveShareAsync(todoListId, targetUserId, userId);
            return this.Ok(new { message = "Share removed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error removing share for todo list {TodoListId}", todoListId);
            throw;
        }
    }

    // Get all users a todo list is shared with
    [HttpGet("todolists/{todoListId}/shared-users")]
    public async Task<IActionResult> GetSharedUsers(int todoListId)
    {
        try
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            var sharedUsers = await this.shareService.GetSharedUsersAsync(todoListId, userId);
            return this.Ok(sharedUsers);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting shared users for todo list {TodoListId}", todoListId);
            throw;
        }
    }

    // Get all todo lists shared with the current user
    [HttpGet("shared-with-me")]
    public async Task<IActionResult> GetSharedWithMe()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return this.Unauthorized();
            }

            var sharedLists = await this.shareService.GetSharedWithMeAsync(userId);
            return this.Ok(sharedLists);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting shared lists for user");
            throw;
        }
    }
}
