using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;
using Microsoft.AspNetCore.Http;

namespace TodoListApp.WebApp.Services;

public class TodoTaskTagWebApiService : ITodoTaskTagWebApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Constructor using IHttpClientFactory style
    public TodoTaskTagWebApiService(IHttpClientFactory httpFactory, IHttpContextAccessor httpContextAccessor)
    {
        _http = httpFactory.CreateClient("WebApiClient"); // already has BaseAddress set in Program.cs
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            throw new InvalidOperationException("JWT token is not available. Please log in first.");
        }
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        AddAuthHeader();
        return await _http.GetFromJsonAsync<List<string>>("api/tasks/tags/all") ?? new List<string>();
    }

    public async Task<List<string>> GetTagsForTaskAsync(int taskId)
    {
        AddAuthHeader();
        return await _http.GetFromJsonAsync<List<string>>($"api/tasks/tags/task/{taskId}") ?? new List<string>();
    }

    public async Task AddTagToTaskAsync(int taskId, string tagName)
    {
        AddAuthHeader();
        var dto = new { TagName = tagName };
        var res = await _http.PostAsJsonAsync($"api/tasks/tags/task/{taskId}", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task RemoveTagFromTaskAsync(int taskId, string tagName)
    {
        AddAuthHeader();
        var res = await _http.DeleteAsync($"api/tasks/tags/task/{taskId}?tagName={tagName}");
        res.EnsureSuccessStatusCode();
    }

    public async Task<List<TodoTaskModel>> GetTasksByTagAsync(string tagName)
    {
        AddAuthHeader();
        return await _http.GetFromJsonAsync<List<TodoTaskModel>>($"api/tasks/tags/bytag/{tagName}") ?? new List<TodoTaskModel>();
    }
}
