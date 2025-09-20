namespace TodoListApp.Services.Database
{
    public class TodoListEntity
    {
        public int Id { get; set; }// Primary key

        public string Name { get; set; } = null!;  // Title of the to-do list

        public string? Description { get; set; }// Optional description

        public string OwnerId { get; set; } = null!; // Foreign key to User (Identity)

        public DateTime CreatedAt { get; set; }// Creation timestamp

        public DateTime? UpdatedAt { get; set; }// Last update timestamp

        // collection for tasks later
    }
}
