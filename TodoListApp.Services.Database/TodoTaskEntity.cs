namespace TodoListApp.Services.Database
{
    public class TodoTaskEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; }

        public int TodoListId { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public string AssignedUserId { get; set; } = string.Empty;

        // Navigation property
        public TodoListEntity TodoList { get; set; } = null!;
    }
}
