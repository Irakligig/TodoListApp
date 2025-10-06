using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.WebApi.Models;
public class TaskWithTagsViewModel : TodoTaskModel
{
    public List<string> Tags { get; set; } = new List<string>();

    public string NewTag { get; set; } = string.Empty; // for adding tags

    public int TodoListId { get; set; } // Parent list ID
}
