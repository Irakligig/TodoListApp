using System.Net.Http.Headers;
using TodoListApp.WebApi.Models;
using static System.Net.WebRequestMethods;

namespace TodoListApp.WebApp.Services;

public class TodoTaskTagWebApiService : ITodoTaskTagWebApiService
{
    private readonly HttpClient http;
    private string? _jwtToken;


    public TodoTaskTagWebApiService(HttpClient client)
    {
        http = client;
    }

    private async Task EnsureTokenAsync()
    {
        if (!string.IsNullOrEmpty(_jwtToken))
        {
            return;
        }

        var tokenResponse = await http.GetFromJsonAsync<TokenResponse>("/api/token");
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.token))
        {
            throw new Exception("Failed to retrieve JWT token from WebAPI.");
        }

        _jwtToken = tokenResponse.token;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        await EnsureTokenAsync();
        return await http.GetFromJsonAsync<List<string>>("api/tasks/tags/all") ?? new List<string>();
    }

    public async Task<List<string>> GetTagsForTaskAsync(int taskId)
    {
        await EnsureTokenAsync();
        return await http.GetFromJsonAsync<List<string>>($"api/tasks/tags/task/{taskId}") ?? new List<string>();
    }

    public async Task AddTagToTaskAsync(int taskId, string tagName)
    {
        await EnsureTokenAsync();
        var dto = new { TagName = tagName };
        await http.PostAsJsonAsync($"api/tasks/tags/task/{taskId}", dto);
    }

    public async Task RemoveTagFromTaskAsync(int taskId, string tagName) {
        await EnsureTokenAsync();
        await http.DeleteAsync($"api/tasks/tags/task/{taskId}?tagName={tagName}");
    }

    public async Task<List<TodoTaskModel>> GetTasksByTagAsync(string tagName)
    {
        await EnsureTokenAsync();
        return await http.GetFromJsonAsync<List<TodoTaskModel>>($"api/tasks/tags/bytag/{tagName}")
        ?? new List<TodoTaskModel>();
    }
}
