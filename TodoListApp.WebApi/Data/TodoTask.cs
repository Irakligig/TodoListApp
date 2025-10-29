namespace TodoListApp.WebApi.Data
{
    public class TodoTask
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; }

        public int TodoListId { get; set; }

        public string? OwnerId { get; set; }

        public string AssignedUserId { get; set; } = string.Empty;

        public List<TodoTag> Tags { get; set; } = new List<TodoTag>();
    }
}
