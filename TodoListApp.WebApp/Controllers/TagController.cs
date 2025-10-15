using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;
[Authorize]
public class TagController : Controller
{
    private readonly ITodoTaskTagWebApiService tagService;

    public TagController(ITodoTaskTagWebApiService tagService)
    {
        tagService = tagService;
    }

    // List all tags
    public async Task<IActionResult> Index()
    {
        var tags = await tagService.GetAllTagsAsync();
        return this.View(tags);
    }

    // Add tag to task
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int taskId, int listId, string newTag)
    {
        var httpUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
        Console.WriteLine($"[DEBUG] AddTag called. taskId={taskId}, newTag={newTag}, httpUserId={httpUserId}");

        if (!string.IsNullOrWhiteSpace(newTag))
        {
            await tagService.AddTagToTaskAsync(taskId, newTag);
        }

        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // Remove tag from task
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int taskId, int listId, string tagName)
    {
        await tagService.RemoveTagFromTaskAsync(taskId, tagName);
        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // List tasks by tag
    public async Task<IActionResult> TasksByTag(string tagName)
    {
        var tasks = await tagService.GetTasksByTagAsync(tagName);
        this.ViewBag.TagName = tagName;
        return this.View(tasks);
    }

    public async Task<IActionResult> AllTags()
    {
        var tags = await tagService.GetAllTagsAsync();
        return this.View(tags);
    }
}
