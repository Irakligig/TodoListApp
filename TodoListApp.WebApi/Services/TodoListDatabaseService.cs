using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Data;

namespace TodoListApp.WebApi.Services;

public class TodoListDatabaseService : ITodoListDatabaseService
{
    private readonly TodoListDbContext context;
    private readonly ILogger<TodoListDatabaseService> logger;

    public TodoListDatabaseService(TodoListDbContext context, ILogger<TodoListDatabaseService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task AddTodoListAsync(TodoList todoList, string ownerId)
    {
        logger.LogInformation("Adding new todo list '{ListName}' for owner {OwnerId}", todoList.Name, ownerId);

        if (string.IsNullOrWhiteSpace(todoList.Name))
        {
            logger.LogError("Todo list name cannot be null or whitespace");
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
        logger.LogInformation("Successfully added todo list {ListId} '{ListName}'", todoList.Id, todoList.Name);
    }

    public async Task DeleteTodoListAsync(int todoListId, string ownerId)
    {
        logger.LogInformation("Deleting todo list {TodoListId} for owner {OwnerId}", todoListId, ownerId);

        var existing = await this.context.TodoLists.FindAsync(todoListId);
        if (existing == null)
        {
            logger.LogWarning("Todo list with Id {TodoListId} not found", todoListId);
            throw new KeyNotFoundException($"Todo list with Id {todoListId} not found.");
        }

        if (existing.OwnerId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have permission to delete todo list {TodoListId}", ownerId, todoListId);
            throw new UnauthorizedAccessException("You do not have permission to delete this todo list.");
        }

        _ = this.context.TodoLists.Remove(existing);
        _ = await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully deleted todo list {TodoListId}", todoListId);
    }

    public async Task<IEnumerable<TodoList>> GetAllTodoListsAsync(string ownerId)
    {
        logger.LogInformation("Getting all todo lists for owner {OwnerId}", ownerId);

        // Return only the lists owned by the logged-in user
        var lists = await this.context.TodoLists
            .Where(t => t.OwnerId == ownerId)
            .Select(t => new TodoList
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                OwnerId = t.OwnerId,
            })
            .ToListAsync();

        logger.LogInformation("Retrieved {ListCount} todo lists for owner {OwnerId}", lists.Count, ownerId);
        return lists;
    }

    public async Task UpdateTodoListAsync(TodoList todoList, string ownerId)
    {
        logger.LogInformation("Updating todo list {TodoListId} for owner {OwnerId}", todoList.Id, ownerId);

        var existing = await this.context.TodoLists.FindAsync(todoList.Id);
        if (existing == null)
        {
            logger.LogWarning("Todo list with Id {TodoListId} not found", todoList.Id);
            throw new KeyNotFoundException($"Todo list with Id {todoList.Id} not found.");
        }

        if (existing.OwnerId != ownerId)
        {
            logger.LogWarning("User {OwnerId} does not have permission to update todo list {TodoListId}", ownerId, todoList.Id);
            throw new UnauthorizedAccessException("You do not have permission to update this todo list.");
        }

        // Update properties
        existing.Name = todoList.Name;
        existing.Description = todoList.Description;

        // OwnerId should not be changed
        _ = await this.context.SaveChangesAsync();
        logger.LogInformation("Successfully updated todo list {TodoListId}", todoList.Id);
    }
}
