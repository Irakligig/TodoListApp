using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class TodoTaskModel
{
    public int Id { get; set; }// Task ID

    [Required]
    public string Name { get; set; } = string.Empty; // Task title

    [Required]
    public string Description { get; set; } = string.Empty; // Task description

    public DateTime? DueDate { get; set; }// Optional due date

    public bool IsCompleted { get; set; }// Completion status

    public int TodoListId { get; set; }// Parent list ID

    public string AssignedUserId { get; set; } = string.Empty; // NEW: user assigned to this task

    public List<string> Tags { get; set; } = new List<string>(); // NEW: associated tags
}
