using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public class TodoListDatabaseService : ITodoListDatabaseService
{
    private readonly TodoListDbContext context;

    public TodoListDatabaseService(TodoListDbContext context)
    {
        this.context = context;
    }

    public async Task AddTodoListAsync(TodoList todoList, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(todoList.Name))
        {
            throw new ArgumentException("Name cannot be null or whitespace");
        }

        // Map DTO to Entity and set the owner from logged-in user
        var entity = new TodoListEntity
        {
            Name = todoList.Name,
            Description = todoList.Description,
            OwnerId = ownerId, // use the userId passed from controller
        };

        _ = await this.context.TodoLists.AddAsync(entity);
        _ = await this.context.SaveChangesAsync();

        // Optionally update DTO with generated Id
        todoList.Id = entity.Id;
    }

    public async Task DeleteTodoListAsync(int todoListId, string ownerId)
    {
        var existing = await this.context.TodoLists.FindAsync(todoListId);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Todo list with Id {todoListId} not found.");
        }

        if (existing.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this todo list.");
        }

        _ = this.context.TodoLists.Remove(existing);
        _ = await this.context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoList>> GetAllTodoListsAsync(string ownerId)
    {
        // Return only the lists owned by the logged-in user
        return await this.context.TodoLists
            .Where(t => t.OwnerId == ownerId)
            .Select(t => new TodoList
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                OwnerId = t.OwnerId,
            })
            .ToListAsync();
    }

    public async Task UpdateTodoListAsync(TodoList todoList, string ownerId)
    {
        var existing = await this.context.TodoLists.FindAsync(todoList.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Todo list with Id {todoList.Id} not found.");
        }

        if (existing.OwnerId != ownerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this todo list.");
        }

        // Update properties
        existing.Name = todoList.Name;
        existing.Description = todoList.Description;

        // OwnerId should not be changed
        _ = await this.context.SaveChangesAsync();
    }
}
