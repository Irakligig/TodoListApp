using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/{taskId:int}/comments")]
    public class TodoCommentController : ControllerBase
    {
        private readonly ITodoCommentDatabaseService _commentService;

        public TodoCommentController(ITodoCommentDatabaseService commentService)
        {
            _commentService = commentService;
        }

        // ✅ GET: api/tasks/{taskId}/comments
        [HttpGet]
        public async Task<ActionResult<List<TodoCommentModel>>> GetComments(int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var comments = await _commentService.GetByTaskIdAsync(taskId, userId);

            var models = comments.Select(c => new TodoCommentModel
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = c.UserName, // Add this line - include username in response
                Text = c.Text,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(models);
        }

        // ✅ POST: api/tasks/{taskId}/comments
        [HttpPost]
        public async Task<ActionResult<TodoCommentModel>> AddComment(int taskId, [FromBody] TodoCommentCreateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown User"; // Get username from claims

            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(model.Text))
            {
                return BadRequest("Comment text is required.");
            }

            // Pass the username to the service
            var comment = await _commentService.AddCommentAsync(taskId, userId, userName, model.Text);

            var result = new TodoCommentModel
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = comment.UserName, // Add this line - include username in response
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };

            return CreatedAtAction(nameof(GetComments), new { taskId }, result);
        }

        // ✅ PUT: api/tasks/{taskId}/comments/{commentId}
        [HttpPut("{commentId:int}")]
        public async Task<IActionResult> EditComment(int commentId, [FromBody] TodoCommentEditModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(model.Text))
            {
                return BadRequest("New text is required.");
            }

            await _commentService.EditCommentAsync(commentId, userId, model.Text);

            return NoContent();
        }

        // ✅ DELETE: api/tasks/{taskId}/comments/{commentId}
        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            await _commentService.DeleteCommentAsync(commentId, userId);

            return NoContent();
        }
    }
}
