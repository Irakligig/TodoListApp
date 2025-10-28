using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TagController : Controller
{
    private readonly ITodoTaskTagWebApiService tagService;
    private readonly IUsersAuthWebApiService authService;

    public TagController(ITodoTaskTagWebApiService tagService, IUsersAuthWebApiService authService)
    {
        this.tagService = tagService;
        this.authService = authService;
    }

    // List all tags
    public async Task<IActionResult> Index()
    {
        if (!authService.IsJwtPresent() || !authService.IsJwtValid())
        {
            return RedirectToAction("Login", "Auth");
        }

        var tags = await tagService.GetAllTagsAsync();
        return this.View(tags);
    }

    // Add tag to task
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTag(int taskId, int listId, string newTag)
    {
        if (!authService.IsJwtPresent() || !authService.IsJwtValid())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(newTag))
            {
                await tagService.AddTagToTaskAsync(taskId, newTag);
                TempData["SuccessMessage"] = $"Tag '{newTag}' added successfully!";
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (ArgumentException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Network error: {ex.Message}";
        }

        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // Remove tag from task
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTag(int taskId, int listId, string tagName)
    {
        if (!authService.IsJwtPresent() || !authService.IsJwtValid())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            await tagService.RemoveTagFromTaskAsync(taskId, tagName);
            TempData["SuccessMessage"] = $"Tag '{tagName}' removed successfully!";
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Network error: {ex.Message}";
        }

        return this.RedirectToAction("Details", "TodoTask", new { listId = listId, id = taskId });
    }

    // List tasks by tag
    public async Task<IActionResult> TasksByTag(string tagName)
    {
        if (!authService.IsJwtPresent() || !authService.IsJwtValid())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var tasks = await tagService.GetTasksByTagAsync(tagName);
            this.ViewBag.TagName = tagName;
            return this.View(tasks);
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Network error: {ex.Message}";
            return this.View(new List<TodoTaskModel>());
        }
    }

    public async Task<IActionResult> AllTags()
    {
        if (!authService.IsJwtPresent() || !authService.IsJwtValid())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var tags = await tagService.GetAllTagsAsync();
            return this.View(tags);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return this.View(new List<string>());
        }
        catch (HttpRequestException ex)
        {
            TempData["ErrorMessage"] = $"Network error: {ex.Message}";
            return this.View(new List<string>());
        }
    }
}
