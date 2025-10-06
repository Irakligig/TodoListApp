using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Services;
using TodoListApp.WebApi.Models;
using System.Security.Claims;

namespace TodoListApp.WebApp.Controllers;

public class TagController : Controller
{
    private readonly ITodoTaskTagWebApiService _tagService;

    public TagController(ITodoTaskTagWebApiService tagService)
    {
        _tagService = tagService;
    }

    // -------------------------
    // List all tags (US18)
    // -------------------------
    public async Task<IActionResult> Index()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return View(tags); // Index.cshtml for all tags
    }

    // -------------------------
    // Add tag to task (US20)
    // -------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int taskId, int listId, string newTag)
    {
        var httpUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        Console.WriteLine($"[DEBUG] AddTag called. taskId={taskId}, newTag={newTag}, httpUserId={httpUserId}");

        if (!string.IsNullOrWhiteSpace(newTag))
        {
            await _tagService.AddTagToTaskAsync(taskId, newTag);
        }

        return RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // -------------------------
    // Remove tag from task (US21)
    // -------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int taskId, int listId, string tagName)
    {
        await _tagService.RemoveTagFromTaskAsync(taskId, tagName);

        // Redirect back to Task Details
        return RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // -------------------------
    // List tasks by tag (US19)
    // -------------------------
    public async Task<IActionResult> TasksByTag(string tagName)
    {
        var taskService = HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
        var tasks = await taskService.GetTasksByTagAsync(tagName);

        ViewBag.TagName = tagName;
        return View(tasks); // pass as List<TodoTaskModel> or your view model
    }

    public async Task<IActionResult> AllTags()
    {
        // Get all tags the user has access to
        var tagService = HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
        var tags = await tagService.GetAllTagsAsync();

        return View(tags); // pass as List<string>
    }

}
