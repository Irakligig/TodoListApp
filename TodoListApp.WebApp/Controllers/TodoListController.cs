using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize]
    public class TodoListController : Controller
    {
        private readonly ITodoListWebApiService todoListService;
        private readonly ITodoTaskWebApiService todoTaskService;
        private readonly IUsersAuthWebApiService authService;

        public TodoListController(
            ITodoListWebApiService todoListService,
            IUsersAuthWebApiService authService,
            ITodoTaskWebApiService todoTaskService)
        {
            this.todoListService = todoListService;
            this.authService = authService;
            this.todoTaskService = todoTaskService;
        }

        public async Task<IActionResult> Index()
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            var lists = await todoListService.GetTodoListsAsync().ConfigureAwait(false);
            var users = await todoTaskService.GetAllUsersAsync();

            // Get shared lists for current user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"Current user ID: {currentUserId}");

            IEnumerable<SharedTodoListDto> sharedLists = new List<SharedTodoListDto>();

            try
            {
                sharedLists = await todoListService.GetSharedWithMeAsync();
                Console.WriteLine($"Successfully retrieved {sharedLists.Count()} shared lists");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP error getting shared lists: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting shared lists: {ex.Message}");
                throw;
            }

            ViewBag.AvailableUsers = users;
            ViewBag.SharedLists = sharedLists;

            // DEBUG: Log what's being passed to view
            Console.WriteLine($"Passing {sharedLists.Count()} shared lists to view");

            // DEBUG: Log each shared list
            foreach (var list in sharedLists)
            {
                Console.WriteLine($"Shared list: {list.TodoListId} - {list.Name} (Role: {list.Role})");
            }

            return View(lists);
        }

        [HttpPost]
        public async Task<IActionResult> ShareList(int listId, string targetUserId, string role)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var success = await todoListService.ShareTodoListAsync(listId, targetUserId, role);
                if (success)
                {
                    TempData["SuccessMessage"] = "List shared successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to share list.";
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to share this list. Only the owner can share lists.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while sharing the list.";
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoListModel list)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            _ = ModelState.Remove(nameof(TodoListModel.OwnerId));

            if (!ModelState.IsValid)
            {
                return View(list);
            }

            list.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "dev-key";

            var success = await todoListService.AddTodoListAsync(list);
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
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await todoListService.DeleteTodoListAsync(id);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this list. Only the owner can delete lists.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the list.";
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ======================
        // Edit TodoList
        // ======================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            var lists = await todoListService.GetTodoListsAsync();
            var list = lists.FirstOrDefault(x => x.Id == id);
            return list == null ? NotFound() : View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TodoListModel list)
        {

            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            _ = ModelState.Remove(nameof(TodoListModel.OwnerId));

            if (!ModelState.IsValid)
            {
                return View(list);
            }

            try
            {
                await todoListService.UpdateTodoListAsync(list);
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this list.";
                return View(list);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SharedWithMe()
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
