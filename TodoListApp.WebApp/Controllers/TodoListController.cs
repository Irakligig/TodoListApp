using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;

public class TodoListController : Controller
{
    private readonly ITodoListWebApiService _todoListService;

    public TodoListController(ITodoListWebApiService todoListService)
    {
        _todoListService = todoListService;
    }

    // ======================
    // List all TodoLists
    // ======================
    public async Task<IActionResult> Index()
    {
        var lists = await _todoListService.GetTodoListsAsync();
        return View(lists);
    }

    // ======================
    // Create TodoList
    // ======================
    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(TodoListModel list)
    {
        _ = ModelState.Remove(nameof(TodoListModel.OwnerId));

        if (!ModelState.IsValid)
        {
            return View(list);
        }

        list.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

        var success = await _todoListService.AddTodoListAsync(list);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Failed to create todo list. Please try again.");
            return View(list);
        }

        return RedirectToAction(nameof(Index));
    }

    // ======================
    // Delete TodoList
    // ======================
    public async Task<IActionResult> Delete(int id)
    {
        await _todoListService.DeleteTodoListAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // ======================
    // Edit TodoList
    // ======================
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lists = await _todoListService.GetTodoListsAsync();
        var list = lists.FirstOrDefault(x => x.Id == id);
        return list == null ? NotFound() : View(list);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TodoListModel list)
    {
        _ = ModelState.Remove(nameof(TodoListModel.OwnerId));

        if (!ModelState.IsValid)
        {
            return View(list);
        }

        await _todoListService.UpdateTodoListAsync(list);
        return RedirectToAction(nameof(Index));
    }
}
