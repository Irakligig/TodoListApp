using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.Services.Database.Entities;
public class TodoTagEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property for many-to-many
    public ICollection<TodoTaskTagEntity> TaskTags { get; set; } = new List<TodoTaskTagEntity>();
}
