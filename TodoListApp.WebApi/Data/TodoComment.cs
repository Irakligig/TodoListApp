namespace TodoListApp.WebApi.Data;

public class TodoComment
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
