using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.Services.Database.Entities;
public class TodoListUser
{
    public int Id { get; set; }

    // Foreign key to TodoList
    public int TodoListId { get; set; }

    // We will NOT use navigation to User here since User is in another DbContext
    public string UserId { get; set; } = string.Empty;

    // Role inside the list (Owner / Editor / Viewer)
    public string Role { get; set; } = "Viewer";
}
