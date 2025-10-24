using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.Services.Database.Entities;
public class TodoCommentEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TaskId { get; set; }// Task that the comment belongs to

    [Required]
    public string UserId { get; set; } = null!; // Author of the comment

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
