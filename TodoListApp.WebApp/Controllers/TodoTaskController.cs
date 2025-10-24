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

            await taskService.CreateAsync(task.TodoListId, task).ConfigureAwait(false);
            return this.RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // GET: /TodoTask/Edit/1034?listId=123
        public async Task<IActionResult> Edit(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

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

        // POST: /TodoTask/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TaskWithTagsViewModel model)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {

                // Print validation errors to console
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

            var addedtag = model.NewTag;
            if (!string.IsNullOrWhiteSpace(addedtag))
            {
                await tagService.AddTagToTaskAsync(model.Id, addedtag);
            }
            // Map ViewModel -> API Model
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

            await taskService.UpdateAsync(model.TodoListId, model.Id, apiModel);

            return RedirectToAction("Index", new { listId = model.TodoListId });
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
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { listId, id = taskId });
            }

            await commentService.AddCommentAsync(taskId, text).ConfigureAwait(false);
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
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("Details", new { listId, id = taskId });
            }

            await commentService.EditCommentAsync(taskId, commentId, text).ConfigureAwait(false);
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

            await commentService.DeleteCommentAsync(taskId, commentId).ConfigureAwait(false);
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

            bool isCompleted = status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
            await taskService.UpdateStatusAsync(taskId, isCompleted);
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

            if (!string.IsNullOrWhiteSpace(newUserId))
            {
                await taskService.ReassignTaskAsync(taskId, newUserId);
            }
            return RedirectToAction("Index");
        }


        // GET: /TodoTask/Details/1046?listId=37
        public async Task<IActionResult> Details(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

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

        // GET: /TodoTask/Delete/1046?listId=37
        public async Task<IActionResult> Delete(int listId, int id)
        {
            if (!authService.IsJwtPresent() || !authService.IsJwtValid())
            {
                return RedirectToAction("Login", "Auth");
            }

            var task = await taskService.GetByIdAsync(listId, id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task); // A simple confirmation page
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
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API error: {ex.Message}";
                return RedirectToAction("Index", "TodoList");
            }
        }

        private async Task<string> GetCurrentApiUserIdAsync()
        {
            var currentUsername = User.Identity?.Name; // "1R3X5" from WebApp Identity

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
                return apiUser.Id; // Returns the API GUID
            }

            // If not found, log warning
            Console.WriteLine($"Warning: No API user found for WebApp user '{currentUsername}'");
            return currentUsername;


        }
    }
}
