namespace TodoListApp.WebApi.Models;
public class ShareRequest
{
    public string TargetUserId { get; set; } = string.Empty;

    public string Role { get; set; } = "Viewer"; // "Viewer" or "Editor"
}
