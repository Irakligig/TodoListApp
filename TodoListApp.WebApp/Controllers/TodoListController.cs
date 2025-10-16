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
        private readonly IUsersAuthWebApiService authService;

        public TodoListController(
            ITodoListWebApiService todoListService,
            IUsersAuthWebApiService authService)
        {
            this.todoListService = todoListService;
            this.authService = authService;
        }


        public async Task<IActionResult> Index()
        {

            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            var lists = await todoListService.GetTodoListsAsync().ConfigureAwait(false);
            return View(lists);
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

            await todoListService.DeleteTodoListAsync(id);
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

            await todoListService.UpdateTodoListAsync(list);
            return RedirectToAction(nameof(Index));
        }
    }
}
