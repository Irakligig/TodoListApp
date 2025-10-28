using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers
{
    [Authorize]
    public class TodoTaskController : Controller
    {
        private readonly ITodoTaskWebApiService taskService;
        private readonly ITodoCommentWebApiService commentService;
        private readonly ITodoTaskTagWebApiService tagService;
        private readonly IUsersAuthWebApiService authService;

        public TodoTaskController(
            ITodoTaskWebApiService taskService,
            ITodoCommentWebApiService commentService,
            ITodoTaskTagWebApiService tagService,
            IUsersAuthWebApiService authService)
        {
            this.taskService = taskService;
            this.commentService = commentService;
            this.tagService = tagService;
            this.authService = authService;
        }

        public async Task<IActionResult> Index(int listId)
        {
            try
            {
                if (!authService.IsJwtPresent() || !authService.IsJwtValid())
                {
                    return RedirectToAction("Login", "Auth");
                }

                var tasks = await taskService.GetTasksAsync(listId).ConfigureAwait(false);
                this.ViewBag.ListId = listId;
                return this.View(tasks);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return this.RedirectToAction("Index", "TodoList");
            }
            catch (System.Security.SecurityException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return this.RedirectToAction("Index", "TodoList");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to view tasks in this list.";
                return this.RedirectToAction("Index", "TodoList");
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"API error: {ex.Message}";
                return this.RedirectToAction("Index", "TodoList");
            }
        }

        public IActionResult Create(int listId)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            this.ViewBag.ListId = listId;
            var model = new TodoTaskModel { TodoListId = listId };
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoTaskModel task)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!this.ModelState.IsValid)
            {
                this.ViewBag.ListId = task.TodoListId;
                return this.View(task);
            }

            try
            {
                await taskService.CreateAsync(task.TodoListId, task).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Task created successfully!";
                return this.RedirectToAction("Index", new { listId = task.TodoListId });
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to create tasks in this list.";
                this.ViewBag.ListId = task.TodoListId;
                return this.View(task);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the task.";
                this.ViewBag.ListId = task.TodoListId;
                return this.View(task);
                throw;
            }
        }

        // GET: /TodoTask/Edit/1034?listId=123
        public async Task<IActionResult> Edit(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var task = await taskService.GetByIdAsync(listId, id);
                if (task == null)
                {
                    return NotFound();
                }

                var tags = await tagService.GetTagsForTaskAsync(id);
                var comments = await commentService.GetCommentsAsync(id);
                var users = await taskService.GetAllUsersAsync();

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

                ViewBag.ListId = listId;
                ViewBag.Users = users;

                return View(model);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this task.";
                return RedirectToAction("Index", new { listId = listId });
            }
        }

        // POST: /TodoTask/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskWithTagsViewModel model)
        {
            Console.WriteLine($"=== DEBUG EDIT POST ===");
            Console.WriteLine($"AssignedUserId from form: '{model.AssignedUserId}'");
            Console.WriteLine($"TaskId: {model.Id}, ListId: {model.TodoListId}");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Log all model state errors
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                }
            }


            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.ListId = model.TodoListId;
                ViewBag.Users = await taskService.GetAllUsersAsync();
                return View(model);
            }

            try
            {
                var apiModel = new TodoTaskModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    IsCompleted = model.IsCompleted,
                    TodoListId = model.TodoListId,
                    AssignedUserId = model.AssignedUserId
                };

                var addedtag = model.NewTag;
                if (!string.IsNullOrWhiteSpace(addedtag))
                {
                    await tagService.AddTagToTaskAsync(model.Id, addedtag);
                }

                await taskService.UpdateAsync(model.TodoListId, model.Id, apiModel);
                TempData["SuccessMessage"] = "Task updated successfully!";
                return RedirectToAction("Index", new { listId = model.TodoListId });
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit tasks in this list.";
                ViewBag.ListId = model.TodoListId;
                ViewBag.Users = await taskService.GetAllUsersAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the task.";
                ViewBag.ListId = model.TodoListId;
                ViewBag.Users = await taskService.GetAllUsersAsync();
                return View(model);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int listId, int taskId, string text)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { listId, id = taskId });
            }

            try
            {
                await commentService.AddCommentAsync(taskId, text).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Comment added successfully!";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to add comments to this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the comment.";
                throw;
            }

            return RedirectToAction("Details", new { listId, id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int listId, int taskId, int commentId, string text)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { listId, id = taskId });
            }

            try
            {
                await commentService.EditCommentAsync(taskId, commentId, text).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Comment updated successfully!";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit comments on this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the comment.";
                throw;
            }

            return RedirectToAction("Details", new { listId, id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int listId, int taskId, int commentId)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await commentService.DeleteCommentAsync(taskId, commentId).ConfigureAwait(false);
                TempData["SuccessMessage"] = "Comment deleted successfully!";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete comments from this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the comment.";
                throw;
            }

            return RedirectToAction("Details", new { listId, id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int taskId, string status)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                bool isCompleted = status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
                await taskService.UpdateStatusAsync(taskId, isCompleted);
                TempData["SuccessMessage"] = "Task status updated successfully!";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to update the status of this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the task status.";
                throw;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignTask(int taskId, string newUserId)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(newUserId))
                {
                    await taskService.ReassignTaskAsync(taskId, newUserId);
                    TempData["SuccessMessage"] = "Task reassigned successfully!";
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to reassign this task.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while reassigning the task.";
                throw;
            }

            return this.RedirectToAction("AssignedTasks");
        }

        // GET: /TodoTask/Details/1046?listId=37
        public async Task<IActionResult> Details(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var task = await taskService.GetByIdAsync(listId, id);
                if (task == null)
                {
                    return NotFound();
                }

                var tags = await tagService.GetTagsForTaskAsync(id);
                var comments = await commentService.GetCommentsAsync(id);
                var users = await taskService.GetAllUsersAsync();

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
                    OwnerId = task.OwnerId,
                };

                ViewBag.ListId = listId;
                ViewBag.Users = users;
                ViewBag.TaskOwnerId = task.OwnerId;
                ViewBag.CurrentUserId = await GetCurrentApiUserIdAsync();
                return View(model);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to view this task.";
                return RedirectToAction("Index", new { listId = listId });
            }
        }

        // GET: /TodoTask/Delete/1046?listId=37
        public async Task<IActionResult> Delete(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var task = await taskService.GetByIdAsync(listId, id);
                if (task == null)
                {
                    return NotFound();
                }

                return View(task);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this task.";
                return RedirectToAction("Index", new { listId = listId });
            }
        }

        // POST: /TodoTask/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await taskService.DeleteAsync(listId, id);
                TempData["SuccessMessage"] = "Task deleted successfully!";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete tasks from this list.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the task.";
                throw;
            }

            return RedirectToAction("Index", new { listId = listId });
        }

        public async Task<IActionResult> AssignedTasks(string? status, string? sortBy)
        {
            try
            {
                if (!authService.IsJwtPresent() || !authService.IsJwtValid())
                {
                    return RedirectToAction("Login", "Auth");
                }

                var tasks = await taskService.GetAssignedAsync(status, sortBy);
                ViewBag.Status = status ?? "";
                ViewBag.SortBy = sortBy ?? "";
                ViewBag.Users = await taskService.GetAllUsersAsync();
                return View(tasks);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("403"))
            {
                TempData["ErrorMessage"] = "You don't have permission to view assigned tasks.";
                return RedirectToAction("Index", "TodoList");
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"API error: {ex.Message}";
                return RedirectToAction("Index", "TodoList");
            }
        }

        private async Task<string> GetCurrentApiUserIdAsync()
        {
            var currentUsername = User.Identity?.Name;

            if (string.IsNullOrEmpty(currentUsername))
            {
                return "unknown-user";
            }

            var apiUsers = await taskService.GetAllUsersAsync();
            var apiUser = apiUsers.FirstOrDefault(u =>
                u.UserName == currentUsername ||
                u.FullName == currentUsername);

            if (apiUser != null)
            {
                return apiUser.Id;
            }

            Console.WriteLine($"Warning: No API user found for WebApp user '{currentUsername}'");
            return currentUsername;
        }
    }
}
