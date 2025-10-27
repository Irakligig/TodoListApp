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
public class TodoListShareController : ControllerBase  // Added : ControllerBase
{
    private readonly ITodoListShareService _shareService;
    private readonly ILogger<TodoListShareController> _logger;

    public TodoListShareController(
        ITodoListShareService shareService,
        ILogger<TodoListShareController> logger)
    {
        _shareService = shareService;
        _logger = logger;
    }

    // Share a specific todo list with a user
    [HttpPost("todolists/{todoListId}/share")]
    public async Task<IActionResult> ShareTodoList(int todoListId, [FromBody] ShareRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _shareService.ShareTodoListAsync(todoListId, request.TargetUserId, request.Role, userId);
            return Ok(new { message = "Todo list shared successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
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
                return Unauthorized();
            }

            await _shareService.UpdateShareAsync(todoListId, targetUserId, request.NewRole, userId);
            return Ok(new { message = "Share updated successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating share for todo list {TodoListId}", todoListId);
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
                return Unauthorized();
            }

            await _shareService.RemoveShareAsync(todoListId, targetUserId, userId);
            return Ok(new { message = "Share removed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing share for todo list {TodoListId}", todoListId);
            throw;
        }
    }

    // Get all users a todo list is shared with
    [HttpGet("todolists/{todoListId}/shared-users")]
    public async Task<IActionResult> GetSharedUsers(int todoListId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sharedUsers = await _shareService.GetSharedUsersAsync(todoListId, userId);
            return Ok(sharedUsers);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.StatusCode(403, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared users for todo list {TodoListId}", todoListId);
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
                return Unauthorized();
            }

            var sharedLists = await _shareService.GetSharedWithMeAsync(userId);
            return Ok(sharedLists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared lists for user");
            throw;
        }
    }
}
