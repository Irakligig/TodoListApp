using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models
{
    public class SearchViewModel
    {
        // Input fields
        public string? Query { get; set; }

        [Display(Name = "Completed")]
        public bool? Status { get; set; }

        [Display(Name = "Due Before")]
        [DataType(DataType.Date)]
        public DateTime? DueBefore { get; set; }

        [Display(Name = "Assigned User ID")]
        public string? AssignedUserId { get; set; }

        // Results
        public List<TodoTaskModel> Results { get; set; } = new List<TodoTaskModel>();
    }
}
