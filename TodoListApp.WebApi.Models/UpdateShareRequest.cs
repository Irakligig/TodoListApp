namespace TodoListApp.WebApi.Models;
public class UpdateShareRequest
{
    public string NewRole { get; set; } = "Viewer"; // "Viewer" or "Editor"
}
