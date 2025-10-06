using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/tags")]
public class TodoTaskTagController : ControllerBase
{
    private readonly ITodoTaskTagDatabaseService tagService;

    public TodoTaskTagController(ITodoTaskTagDatabaseService tagService)
    {
        this.tagService = tagService;
    }

    // GET: api/tasks/tags/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAllTags()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        var tags = await tagService.GetAllTagsAsync(userId);
        return Ok(tags);
    }

    // GET: api/tasks/tags/task/123
    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetTagsForTask(int taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        Console.WriteLine($"[DEBUG] GetTagsForTask called. taskId={taskId}, userId={userId}");

        try
        {
            var tags = await tagService.GetTagsForTaskAsync(taskId, userId);
            Console.WriteLine($"[DEBUG] Tags returned: {string.Join(",", tags)}");
            return Ok(tags);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("[DEBUG] Task not found or access denied.");
            return NotFound($"Task {taskId} not found or access denied.");
        }
    }

    // POST: api/tasks/tags/task/123
    [HttpPost("task/{taskId}")]
    public async Task<IActionResult> AddTagToTask(int taskId, [FromBody] AddTagDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            await tagService.AddTagToTaskAsync(taskId, dto.TagName, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // DELETE: api/tasks/tags/task/123?tagName=urgent
    [HttpDelete("task/{taskId}")]
    public async Task<IActionResult> RemoveTagFromTask(int taskId, [FromQuery] string tagName)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            await tagService.RemoveTagFromTaskAsync(taskId, tagName, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // GET: api/tasks/tags/bytag/urgent
    [HttpGet("bytag/{tagName}")]
    public async Task<IActionResult> GetTasksByTag(string tagName)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        var tasks = await tagService.GetTasksByTagAsync(userId, tagName);
        return Ok(tasks);
    }
}

// DTO for POST body
public class AddTagDto
{
    public string TagName { get; set; } = string.Empty;
}

