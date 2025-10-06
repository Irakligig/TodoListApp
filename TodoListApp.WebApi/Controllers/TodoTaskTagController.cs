using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        var tags = await this.tagService.GetAllTagsAsync(userId);
        return this.Ok(tags);
    }

    // GET: api/tasks/tags/task/123
    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetTagsForTask(int taskId)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        Console.WriteLine($"[DEBUG] GetTagsForTask called. taskId={taskId}, userId={userId}");

        try
        {
            var tags = await this.tagService.GetTagsForTaskAsync(taskId, userId);
            Console.WriteLine($"[DEBUG] Tags returned: {string.Join(",", tags)}");
            return this.Ok(tags);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("[DEBUG] Task not found or access denied.");
            return this.NotFound($"Task {taskId} not found or access denied.");
        }
    }

    // POST: api/tasks/tags/task/123
    [HttpPost("task/{taskId}")]
    public async Task<IActionResult> AddTagToTask(int taskId, [FromBody] AddTagDto dto)
    {
        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            await this.tagService.AddTagToTaskAsync(taskId, dto.TagName, userId);
            return this.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Forbid(ex.Message);
        }
    }

    // DELETE: api/tasks/tags/task/123?tagName=urgent
    [HttpDelete("task/{taskId}")]
    public async Task<IActionResult> RemoveTagFromTask(int taskId, [FromQuery] string tagName)
    {
        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
            await this.tagService.RemoveTagFromTaskAsync(taskId, tagName, userId);
            return this.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return this.NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return this.Forbid(ex.Message);
        }
    }

    // GET: api/tasks/tags/bytag/urgent
    [HttpGet("bytag/{tagName}")]
    public async Task<IActionResult> GetTasksByTag(string tagName)
    {
        var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        var tasks = await this.tagService.GetTasksByTagAsync(userId, tagName);
        return this.Ok(tasks);
    }
}

// DTO for POST body
public class AddTagDto
{
    public string TagName { get; set; } = string.Empty;
}
