using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services;

public class TodoCommentWebApiService : ITodoCommentWebApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TodoCommentWebApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["JwtToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }


    public async Task<IEnumerable<TodoCommentModel>> GetCommentsAsync(int taskId)
    {
        AddAuthHeader();

        var res = await _http.GetAsync($"/api/tasks/{taskId}/comments");

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to get comments: {res.StatusCode}");
        }

        var comments = await res.Content.ReadFromJsonAsync<IEnumerable<TodoCommentModel>>();

        if (comments == null)
        {
            return Array.Empty<TodoCommentModel>();
        }

        return comments;
    }

    public async Task AddCommentAsync(int taskId, string text)
    {
        AddAuthHeader();

        var model = new TodoCommentCreateModel { Text = text };
        var res = await _http.PostAsJsonAsync($"/api/tasks/{taskId}/comments", model);

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to add comment: {res.StatusCode}");
        }
    }

    public async Task EditCommentAsync(int taskId, int commentId, string newText)
    {
        AddAuthHeader();

        var model = new TodoCommentEditModel { Text = newText };
        var res = await _http.PutAsJsonAsync($"/api/tasks/{taskId}/comments/{commentId}", model);

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to edit comment: {res.StatusCode}");
        }
    }

    public async Task DeleteCommentAsync(int taskId, int commentId)
    {
        AddAuthHeader();

        var res = await _http.DeleteAsync($"/api/tasks/{taskId}/comments/{commentId}");

        if (!res.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to delete comment: {res.StatusCode}");
        }
    }
}
