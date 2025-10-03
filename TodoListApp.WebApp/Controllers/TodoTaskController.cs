using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class TodoTaskController : Controller
    {
        private readonly ITodoTaskWebApiService _taskService;

        public TodoTaskController(ITodoTaskWebApiService taskService)
        {
            _taskService = taskService;
        }

        // ======================
        // List all tasks in a TodoList
        // ======================
        public async Task<IActionResult> Index(int listId)
        {
            try
            {
                var tasks = await _taskService.GetTasksAsync(listId);
                ViewBag.ListId = listId;
                return View(tasks);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "TodoList");
            }
            catch (System.Security.SecurityException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "TodoList");
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API error: {ex.Message}";
                return RedirectToAction("Index", "TodoList");
            }
        }

        // ======================
        // Create Task
        // ======================
        public IActionResult Create(int listId)
        {
            ViewBag.ListId = listId;
            var model = new TodoTaskModel { TodoListId = listId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoTaskModel task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ListId = task.TodoListId;
                return View(task);
            }

            await _taskService.CreateAsync(task.TodoListId, task);
            return RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // ======================
        // Edit Task
        // ======================
        public async Task<IActionResult> Edit(int listId, int id)
        {
            var task = await _taskService.GetByIdAsync(listId, id);
            if (task == null)
            {
                return NotFound();
            }

            // Get all users for dropdown
            ViewBag.Users = await _taskService.GetAllUsersAsync(); // Returns List<UserModel>

            ViewBag.ListId = task.TodoListId;
            return View(task);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TodoTaskModel task)
        {
            if (!ModelState.IsValid)
            {
                return View(task);
            }

            await _taskService.UpdateAsync(task.TodoListId, task.Id, task);
            return RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // ======================
        // Delete Task
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int listId, int id)
        {
            await _taskService.DeleteAsync(listId, id);
            return RedirectToAction("Index", new { listId });
        }

        // ======================
        // Assigned Tasks
        // ======================
        public async Task<IActionResult> AssignedTasks(string? status, string? sortBy)
        {
            var tasks = await _taskService.GetAssignedAsync(status, sortBy);
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Users = (await _taskService.GetAllUsersAsync())
                            .Where(u => u.Id != currentUserId)
                            .ToList();


            return View(tasks);
        }

        // ======================
        // Update Status
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            bool isCompleted = status == "Completed";
            await _taskService.UpdateStatusAsync(id, isCompleted);
            return RedirectToAction(nameof(AssignedTasks));
        }

        // ======================
        // Task Details
        // ======================
        public async Task<IActionResult> Details(int listId, int id)
        {
            var task = await _taskService.GetByIdAsync(listId, id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.ListId = listId;
            return View(task);
        }

        // ======================
        // Reassign Task
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignTask(int taskId, string newUserId)
        {
            try
            {
                await _taskService.ReassignTaskAsync(taskId, newUserId);
                TempData["Success"] = "Task reassigned successfully";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You are not allowed to reassign this task.";
            }

            return RedirectToAction(nameof(AssignedTasks));
        }
    }
}
