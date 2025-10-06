using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Services;
using TodoListApp.WebApi.Models;
using System.Security.Claims;

namespace TodoListApp.WebApp.Controllers;

public class TagController : Controller
{
    private readonly ITodoTaskTagWebApiService tagService;

    public TagController(ITodoTaskTagWebApiService tagService)
    {
        this.tagService = tagService;
    }

    // -------------------------
    // List all tags (US18)
    // -------------------------
    public async Task<IActionResult> Index()
    {
        var tags = await this.tagService.GetAllTagsAsync();
        return this.View(tags); // Index.cshtml for all tags
    }

    // -------------------------
    // Add tag to task (US20)
    // -------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int taskId, int listId, string newTag)
    {
        var httpUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        Console.WriteLine($"[DEBUG] AddTag called. taskId={taskId}, newTag={newTag}, httpUserId={httpUserId}");

        if (!string.IsNullOrWhiteSpace(newTag))
        {
            await this.tagService.AddTagToTaskAsync(taskId, newTag);
        }

        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // -------------------------
    // Remove tag from task (US21)
    // -------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int taskId, int listId, string tagName)
    {
        await this.tagService.RemoveTagFromTaskAsync(taskId, tagName);

        // Redirect back to Task Details
        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // -------------------------
    // List tasks by tag (US19)
    // -------------------------
    public async Task<IActionResult> TasksByTag(string tagName)
    {
        var taskService = this.HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
        var tasks = await taskService.GetTasksByTagAsync(tagName);

        this.ViewBag.TagName = tagName;
        return this.View(tasks); // pass as List<TodoTaskModel> or your view model
    }

    public async Task<IActionResult> AllTags()
    {
        // Get all tags the user has access to
        var tagService = this.HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
        var tags = await tagService.GetAllTagsAsync();

        return this.View(tags); // pass as List<string>
    }

}
