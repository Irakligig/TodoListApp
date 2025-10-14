using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.WebApi.Models;
    public class RegisterModel
    {
    [Required]
    [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
        public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }

}
