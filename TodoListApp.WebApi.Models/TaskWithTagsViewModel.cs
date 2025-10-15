using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoListApp.WebApi.Models;
public class TaskWithTagsViewModel : TodoTaskModel
{
    public string? NewTag { get; set; } // for adding tags

}
