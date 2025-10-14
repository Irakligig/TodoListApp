using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TodoListApp.Services.Database.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }
    }
}
