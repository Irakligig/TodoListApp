using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities
{
    public class User
    {
        [Key]
        public string Id { get; set; } = null!; // Guid or string (for identity)

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        public string FullName { get; set; } = null!;
    }
}
