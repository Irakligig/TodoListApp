using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class TodoListController : Controller
    {
        private readonly ITodoListWebApiService _todoListService;

        public TodoListController(ITodoListWebApiService todoListService)
        {
            _todoListService = todoListService;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine("Index hit!");
            var lists = await _todoListService.GetTodoListsAsync();
            Console.WriteLine($"Got {lists.Count()} lists");
            return View(lists);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(TodoListModel list)
        {
            // Remove OwnerId from ModelState entirely before validation
            ModelState.Remove("OwnerId");

            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Assign the current logged-in user as the owner
                    list.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
                    Console.WriteLine($"Creating todo list for user: {list.OwnerId}");

                    await _todoListService.AddTodoListAsync(list);
                    Console.WriteLine("Todo list created successfully, redirecting to Index");

                    return RedirectToAction(nameof(Index));
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Error creating todo list: {ex.Message}");
                    ModelState.AddModelError("", $"Error creating todo list: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Invalid operation error: {ex.Message}");
                    ModelState.AddModelError("", $"Error creating todo list: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState errors:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($" - {key}: {error.ErrorMessage}");
                    }
                }
            }

            return View(list);
        }

        // US03: Delete
        public async Task<IActionResult> Delete(int id)
        {
            await _todoListService.DeleteTodoListAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // US04: Update
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lists = await _todoListService.GetTodoListsAsync();
            var list = lists.FirstOrDefault(x => x.Id == id);
            if (list == null)
            {
                return NotFound();
            }
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TodoListModel list)
        {
            if (ModelState.IsValid)
            {
                await _todoListService.UpdateTodoListAsync(list);
                return RedirectToAction(nameof(Index));
            }
            return View(list);
        }
    }
}
