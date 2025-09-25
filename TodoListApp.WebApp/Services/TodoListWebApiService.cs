using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public class TodoListWebApiService : ITodoListWebApiService
{
    private readonly HttpClient httpClient;

    public TodoListWebApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<TodoListModel>> GetTodoListsAsync()
    {
        return await this.httpClient.GetFromJsonAsync<IEnumerable<TodoListModel>>("api/todolists")
               ?? new List<TodoListModel>();
    }

    public async Task<TodoListModel?> AddTodoListAsync(TodoListModel newList)
    {
        var response = await this.httpClient.PostAsJsonAsync("api/todolists", newList);
        return await response.Content.ReadFromJsonAsync<TodoListModel>();
    }

    public async Task<bool> DeleteTodoListAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"api/todolists/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<TodoListModel?> UpdateTodoListAsync(TodoListModel updatedList)
    {
        var response = await this.httpClient.PutAsJsonAsync($"api/todolists/{updatedList.Id}", updatedList);
        return await response.Content.ReadFromJsonAsync<TodoListModel>();
    }
}
