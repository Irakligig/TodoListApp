using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.WebApi.Models;
public class TaskWithTagsViewModel : TodoTaskModel
{
    public string NewTag { get; set; } = string.Empty; // for adding tags

    public new int TodoListId { get; set; } // Parent list ID
}
