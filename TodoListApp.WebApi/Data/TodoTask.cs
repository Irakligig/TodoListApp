namespace TodoListApp.WebApi.Data
{
    public class TodoTask
    {
        public int Id { get; set; }// Task ID

        public string Name { get; set; } = string.Empty; // Task title

        public string Description { get; set; } = string.Empty; // Task description

        public DateTime? DueDate { get; set; }// Optional due date

        public bool IsCompleted { get; set; }// Completion status

        public int TodoListId { get; set; }// Parent TodoList ID

        public string? OwnerId { get; set; } // Owner of the task

        public string AssignedUserId { get; set; } = string.Empty; // User assigned to this task

        public List<TodoTag> Tags { get; set; } = new List<TodoTag>(); // Associated tags
    }
}
