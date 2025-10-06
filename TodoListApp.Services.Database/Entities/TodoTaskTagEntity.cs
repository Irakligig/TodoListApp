using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.Services.Database.Entities;
public class TodoTaskTagEntity
{
    public int TodoTaskId { get; set; }

    public TodoTaskEntity TodoTask { get; set; } = null!;

    public int TagId { get; set; }

    public TodoTagEntity Tag { get; set; } = null!;
}
