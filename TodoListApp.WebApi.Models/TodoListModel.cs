using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models;

public class TodoListModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
