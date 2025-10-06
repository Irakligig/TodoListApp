using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class TodoTaskController : Controller
    {
        private readonly ITodoTaskWebApiService taskService;

        public TodoTaskController(ITodoTaskWebApiService taskService)
        {
            this.taskService = taskService;
        }

        // ======================
        // List all tasks in a TodoList
        // ======================
        public async Task<IActionResult> Index(int listId)
        {
            try
            {
                var tasks = await this.taskService.GetTasksAsync(listId).ConfigureAwait(false);
                this.ViewBag.ListId = listId;
                return this.View(tasks);
            }
            catch (KeyNotFoundException ex)
            {
                this.TempData["Error"] = ex.Message;
                return this.RedirectToAction("Index", "TodoList");
            }
            catch (System.Security.SecurityException ex)
            {
                this.TempData["Error"] = ex.Message;
                return this.RedirectToAction("Index", "TodoList");
            }
            catch (HttpRequestException ex)
            {
                this.TempData["Error"] = $"API error: {ex.Message}";
                return this.RedirectToAction("Index", "TodoList");
            }
        }

        // ======================
        // Create Task
        // ======================
        public IActionResult Create(int listId)
        {
            this.ViewBag.ListId = listId;
            var model = new TodoTaskModel { TodoListId = listId };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoTaskModel task)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewBag.ListId = task.TodoListId;
                return this.View(task);
            }

            _ = await this.taskService.CreateAsync(task.TodoListId, task).ConfigureAwait(false);
            return this.RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // ======================
        // Edit Task
        // ======================
        public async Task<IActionResult> Edit(int listId, int id)
        {
            var task = await this.taskService.GetByIdAsync(listId, id).ConfigureAwait(false);
            if (task == null)
            {
                return this.NotFound();
            }

            // Get all users for dropdown
            this.ViewBag.Users = await this.taskService.GetAllUsersAsync().ConfigureAwait(false); // Returns List<UserModel>

            this.ViewBag.ListId = task.TodoListId;
            return this.View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TodoTaskModel task)
        {
            Console.WriteLine($"POST Edit: Id={task.Id}, ListId={task.TodoListId}, Name={task.Name}");

            if (!this.ModelState.IsValid)
            {
                Console.WriteLine("ModelState invalid!");
                return this.View(task);
            }

            try
            {
                Console.WriteLine("Before UpdateAsync");
                await this.taskService.UpdateAsync(task.TodoListId, task.Id, task).ConfigureAwait(false);
                Console.WriteLine("After UpdateAsync");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in UpdateAsync: " + ex.Message);
                throw;
            }

            return this.RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // ======================
        // Delete Task
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int listId, int id)
        {
            await this.taskService.DeleteAsync(listId, id).ConfigureAwait(false);
            return this.RedirectToAction("Index", new { listId });
        }

        // ======================
        // Assigned Tasks
        // ======================
        public async Task<IActionResult> AssignedTasks(string? status, string? sortBy)
        {
            var tasks = await this.taskService.GetAssignedAsync(status, sortBy).ConfigureAwait(false);
            this.ViewBag.Status = status;
            this.ViewBag.SortBy = sortBy;

            var currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            this.ViewBag.Users = (await this.taskService.GetAllUsersAsync().ConfigureAwait(false))
                            .Where(u => u.Id != currentUserId)
                            .ToList();

            return this.View(tasks);
        }

        // ======================
        // Update Status
        // ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            bool isCompleted = status == "Completed";
            await this.taskService.UpdateStatusAsync(id, isCompleted).ConfigureAwait(false);
            return this.RedirectToAction(nameof(this.AssignedTasks));
        }

        // ======================
        // Task Details
        // ======================
        public async Task<IActionResult> Details(int listId, int id)
        {
            var task = await this.taskService.GetByIdAsync(listId, id).ConfigureAwait(false);
            if (task == null)
            {
                return this.NotFound();
            }

            // Fetch tags for this task
            var tagService = this.HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
            var tags = await tagService.GetTagsForTaskAsync(id).ConfigureAwait(false);

            // Use TaskWithTagsViewModel
            var model = new TaskWithTagsViewModel
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                TodoListId = task.TodoListId,
                AssignedUserId = task.AssignedUserId,
                Tags = tags,
                NewTag = string.Empty, // For adding new tag
            };

            this.ViewBag.ListId = listId;
            return this.View(model);
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
                await this.taskService.ReassignTaskAsync(taskId, newUserId).ConfigureAwait(false);
                this.TempData["Success"] = "Task reassigned successfully";
            }
            catch (KeyNotFoundException ex)
            {
                this.TempData["Error"] = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                this.TempData["Error"] = "You are not allowed to reassign this task.";
            }

            return this.RedirectToAction(nameof(this.AssignedTasks));
        }
    }
}
