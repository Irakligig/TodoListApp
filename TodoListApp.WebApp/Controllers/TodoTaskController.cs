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

        public TodoTaskController(
            ITodoTaskWebApiService taskService,
            ITodoCommentWebApiService commentService,
            ITodoTaskTagWebApiService tagService)
        {
            taskService = taskService;
            commentService = commentService;
            tagService = tagService;
        }

        public async Task<IActionResult> Index(int listId)
        {
            try
            {
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

            await _taskService.CreateAsync(task.TodoListId, task).ConfigureAwait(false);
            return this.RedirectToAction("Index", new { listId = task.TodoListId });
        }

        // GET: /TodoTask/Edit/1034?listId=123
        public async Task<IActionResult> Edit(int listId, int id)
        {
            var task = await _taskService.GetByIdAsync(listId, id);
            if (task == null)
            {
                return NotFound();
            }

            var tags = await _tagService.GetTagsForTaskAsync(id);
            var comments = await _commentService.GetCommentsAsync(id);
            var users = await _taskService.GetAllUsersAsync();

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
            if (!ModelState.IsValid)
            {
                ViewBag.ListId = model.TodoListId;
                ViewBag.Users = await _taskService.GetAllUsersAsync();
                return View(model);
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

            await _taskService.UpdateAsync(model.TodoListId, model.Id, apiModel);

            return RedirectToAction("Index", new { listId = model.TodoListId });
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

            await _commentService.AddCommentAsync(taskId, text).ConfigureAwait(false);
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

            await _commentService.EditCommentAsync(taskId, commentId, text).ConfigureAwait(false);
            return RedirectToAction("Edit", new { listId, id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int listId, int taskId, int commentId)
        {
            await _commentService.DeleteCommentAsync(taskId, commentId).ConfigureAwait(false);
            return RedirectToAction("Edit", new { listId, id = taskId });
        }
    }
