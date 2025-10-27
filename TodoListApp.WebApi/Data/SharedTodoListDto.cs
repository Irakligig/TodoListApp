namespace TodoListApp.WebApi.Data;

public class SharedTodoListDto
{
    public int TodoListId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime SharedAt { get; set; }
}
