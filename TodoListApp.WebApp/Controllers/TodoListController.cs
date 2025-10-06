using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class TodoListController : Controller
    {
        private readonly ITodoListWebApiService todoListService;

        public TodoListController(ITodoListWebApiService todoListService)
        {
            this.todoListService = todoListService;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine("Index hit!");
            var lists = await this.todoListService.GetTodoListsAsync().ConfigureAwait(false);
            Console.WriteLine($"Got {lists.Count()} lists");
            return this.View(lists);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoListModel list)
        {
            // Remove OwnerId from ModelState entirely before validation
            _ = this.ModelState.Remove(nameof(TodoListModel.OwnerId));

            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            if (this.ModelState.IsValid)
            {
                try
                {
                    // Assign the current logged-in user as the owner
                    list.OwnerId = this.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";
                    Console.WriteLine($"Creating todo list for user: {list.OwnerId}");

                    await this.todoListService.AddTodoListAsync(list).ConfigureAwait(false);
                    Console.WriteLine("Todo list created successfully, redirecting to Index");

                    return this.RedirectToAction(nameof(this.Index));
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP Error creating todo list: {ex.Message}");
                    this.ModelState.AddModelError(string.Empty, $"Error creating todo list: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Invalid operation error: {ex.Message}");
                    this.ModelState.AddModelError(string.Empty, $"Error creating todo list: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("ModelState errors:");
                foreach (var key in this.ModelState.Keys)
                {
                    var state = this.ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($" - {key}: {error.ErrorMessage}");
                    }
                }
            }

            return this.View(list);
        }

        // US03: Delete
        public async Task<IActionResult> Delete(int id)
        {
            await this.todoListService.DeleteTodoListAsync(id).ConfigureAwait(false);
            return this.RedirectToAction(nameof(this.Index));
        }

        // US04: Update
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lists = await this.todoListService.GetTodoListsAsync().ConfigureAwait(false);
            var list = lists.FirstOrDefault(x => x.Id == id);
            if (list == null)
            {
                return this.NotFound();
            }

            return this.View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TodoListModel list)
        {
            _ = this.ModelState.Remove(nameof(TodoListModel.OwnerId));

            if (this.ModelState.IsValid)
            {
                await this.todoListService.UpdateTodoListAsync(list).ConfigureAwait(false);
                return this.RedirectToAction(nameof(this.Index));
            }

            return this.View(list);
        }
    }
}
