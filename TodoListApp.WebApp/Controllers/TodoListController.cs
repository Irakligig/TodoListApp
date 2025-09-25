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
            var lists = await _todoListService.GetTodoListsAsync();
            return View(lists);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(TodoListModel list)
        {
            if (ModelState.IsValid)
            {
                await _todoListService.AddTodoListAsync(list);
                return RedirectToAction(nameof(Index));
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
