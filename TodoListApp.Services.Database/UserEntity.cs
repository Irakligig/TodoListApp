using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.Services.Database
{
    public class User
    {
        [Key]
        public string Id { get; set; } = null!; // Guid or string (for identity)
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;// optional
    }

}
