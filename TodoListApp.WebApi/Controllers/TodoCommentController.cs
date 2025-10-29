using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/{taskId:int}/comments")]
    public class TodoCommentController : ControllerBase
    {
        private readonly ITodoCommentDatabaseService commentService;

        public TodoCommentController(ITodoCommentDatabaseService commentService)
        {
            this.commentService = commentService;
        }

        // ✅ GET: api/tasks/{taskId}/comments
        [HttpGet]
        public async Task<ActionResult<List<TodoCommentModel>>> GetComments(int taskId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return this.Unauthorized();
            }

            var comments = await this.commentService.GetByTaskIdAsync(taskId, userId);

            var models = comments.Select(c => new TodoCommentModel
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = c.UserName, // Add this line - include username in response
                Text = c.Text,
                CreatedAt = c.CreatedAt,
            }).ToList();

            return this.Ok(models);
        }

        // ✅ POST: api/tasks/{taskId}/comments
        [HttpPost]
        public async Task<ActionResult<TodoCommentModel>> AddComment(int taskId, [FromBody] TodoCommentCreateModel model)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = this.User.FindFirstValue(ClaimTypes.Name) ?? "Unknown User"; 

            if (userId == null)
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(model.Text))
            {
                return this.BadRequest("Comment text is required.");
            }

            // Pass the username to the service
            var comment = await this.commentService.AddCommentAsync(taskId, userId, userName, model.Text);

            var result = new TodoCommentModel
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = comment.UserName, // Add this line - include username in response
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
            };

            return this.CreatedAtAction(nameof(this.GetComments), new { taskId }, result);
        }

        // ✅ PUT: api/tasks/{taskId}/comments/{commentId}
        [HttpPut("{commentId:int}")]
        public async Task<IActionResult> EditComment(int commentId, [FromBody] TodoCommentEditModel model)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(model.Text))
            {
                return this.BadRequest("New text is required.");
            }

            await this.commentService.EditCommentAsync(commentId, userId, model.Text);

            return this.NoContent();
        }

        // ✅ DELETE: api/tasks/{taskId}/comments/{commentId}
        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return this.Unauthorized();
            }

            await this.commentService.DeleteCommentAsync(commentId, userId);

            return this.NoContent();
        }
    }
}
