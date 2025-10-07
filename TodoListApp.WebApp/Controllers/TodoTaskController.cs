using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    public class TodoTaskController : Controller
    {
        private readonly ITodoTaskWebApiService taskService;
        private readonly ITodoCommentWebApiService commentService;

        public TodoTaskController(ITodoTaskWebApiService taskService, ITodoCommentWebApiService commentService)
        {
            this.taskService = taskService;
            this.commentService = commentService;
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

            var tagService = HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
            var tags = await tagService.GetTagsForTaskAsync(id);

            var comments = await this.commentService.GetCommentsAsync(id);

            this.ViewBag.Users = await this.taskService.GetAllUsersAsync().ConfigureAwait(false);

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
                Comments = comments.ToList(),
                NewTag = string.Empty
            };

            this.ViewBag.ListId = listId;
            return this.View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskWithTagsViewModel model)
        {
            Console.WriteLine($"POST Edit: Id={model.Id}, ListId={model.TodoListId}, Name={model.Name}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState invalid!");

                // Re-populate tags and comments
                var tagService = HttpContext.RequestServices.GetRequiredService<ITodoTaskTagWebApiService>();
                model.Tags = await tagService.GetTagsForTaskAsync(model.Id);
                model.Comments = (await this.commentService.GetCommentsAsync(model.Id)).ToList();

                // Re-populate users for dropdown
                this.ViewBag.Users = await this.taskService.GetAllUsersAsync().ConfigureAwait(false);

                return View(model);
            }

            try
            {
                Console.WriteLine("Before UpdateAsync");

                var updatedTask = new TodoTaskModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    IsCompleted = model.IsCompleted,
                    TodoListId = model.TodoListId,
                    AssignedUserId = model.AssignedUserId
                };

                await this.taskService.UpdateAsync(model.TodoListId, model.Id, updatedTask).ConfigureAwait(false);

                Console.WriteLine("After UpdateAsync");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in UpdateAsync: " + ex.Message);
                throw;
            }

            return RedirectToAction("Details", new { listId = model.TodoListId, id = model.Id });
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

            // Fetch comments for this task
            var commentService = this.HttpContext.RequestServices.GetRequiredService<ITodoCommentWebApiService>();
            var comments = await commentService.GetCommentsAsync(id).ConfigureAwait(false);

            // Build the ViewModel
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
                Comments = comments.ToList(),
                NewTag = string.Empty,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int listId, int taskId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Edit", new { listId, id = taskId });
            }

            // Add comment using the service
            await commentService.AddCommentAsync(taskId, text);

            // Redirect to the Edit page with correct listId
            return RedirectToAction("Edit", new { listId, id = taskId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int listId, int taskId, int commentId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Edit", new { listId, id = taskId });
            }

            await commentService.EditCommentAsync(taskId, commentId, text);

            return RedirectToAction("Edit", new { listId, id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int listId, int taskId, int commentId)
        {
            await commentService.DeleteCommentAsync(taskId, commentId);

            return RedirectToAction("Edit", new { listId, id = taskId });
        }


    }
}
