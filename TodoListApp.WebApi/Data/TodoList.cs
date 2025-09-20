namespace TodoListApp.WebApi.Data;

public class TodoList
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string OwnerId { get; set; } = null!;
}
