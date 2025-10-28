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

    public TodoTaskTagWebApiService(IHttpClientFactory httpFactory, IHttpContextAccessor httpContextAccessor)
    {
        _http = httpFactory.CreateClient("WebApiClient");
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
        var response = await _http.GetAsync("api/tasks/tags/all");

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Authentication required to access tags.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
    }

    public async Task<List<string>> GetTagsForTaskAsync(int taskId)
    {
        AddAuthHeader();
        var response = await _http.GetAsync($"api/tasks/tags/task/{taskId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
    }

    public async Task AddTagToTaskAsync(int taskId, string tagName)
    {
        AddAuthHeader();
        var dto = new { TagName = tagName };
        var response = await _http.PostAsJsonAsync($"api/tasks/tags/task/{taskId}", dto);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("You don't have permission to add tags to this task. Only the task owner can modify tags.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} not found.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new ArgumentException($"Invalid tag name: {tagName}");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveTagFromTaskAsync(int taskId, string tagName)
    {
        AddAuthHeader();
        var response = await _http.DeleteAsync($"api/tasks/tags/task/{taskId}?tagName={Uri.EscapeDataString(tagName)}");

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("You don't have permission to remove tags from this task. Only the task owner can modify tags.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Task with ID {taskId} or tag '{tagName}' not found.");
        }

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TodoTaskModel>> GetTasksByTagAsync(string tagName)
    {
        AddAuthHeader();
        var response = await _http.GetAsync($"api/tasks/tags/bytag/{Uri.EscapeDataString(tagName)}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Return empty list instead of throwing for "not found" - it's not really an error
            return new List<TodoTaskModel>();
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TodoTaskModel>>() ?? new List<TodoTaskModel>();
    }
}
