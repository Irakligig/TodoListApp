using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.WebApi.Models
{
    public class CollaboratorModel
    {
        public string UserId { get; set; } = null!;

        public string Role { get; set; } = "Editor"; // default to Editor
    }
}
