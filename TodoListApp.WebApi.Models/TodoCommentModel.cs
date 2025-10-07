namespace TodoListApp.WebApi.Models
{
    public class TodoCommentModel
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; } // optional: display name in UI

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
