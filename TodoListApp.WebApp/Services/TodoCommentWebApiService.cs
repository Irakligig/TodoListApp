using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoCommentWebApiService : ITodoCommentWebApiService
    {
        private readonly HttpClient _http;
        private string? _jwtToken;

        public TodoCommentWebApiService(HttpClient http)
        {
            _http = http;
        }

        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_jwtToken))
            {
                return;
            }

            var tokenResponse = await _http.GetFromJsonAsync<TokenResponse>("/api/token");
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.token))
            {
                throw new Exception("Failed to retrieve JWT token from WebAPI.");
            }

            _jwtToken = tokenResponse.token;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        // ✅ Get all comments for a task
        public async Task<IEnumerable<TodoCommentModel>> GetCommentsAsync(int taskId)
        {
            await EnsureTokenAsync();

            var res = await _http.GetAsync($"/api/tasks/{taskId}/comments");
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoCommentModel>>() ?? Array.Empty<TodoCommentModel>();
        }

        // ✅ Add comment
        public async Task AddCommentAsync(int taskId, string text)
        {
            await EnsureTokenAsync();

            var model = new TodoCommentCreateModel { Text = text };
            var res = await _http.PostAsJsonAsync($"/api/tasks/{taskId}/comments", model);
            res.EnsureSuccessStatusCode();
        }

        // ✅ Edit comment
        public async Task EditCommentAsync(int taskId, int commentId, string newText)
        {
            await EnsureTokenAsync();

            var model = new TodoCommentEditModel { Text = newText };
            var res = await _http.PutAsJsonAsync($"/api/tasks/{taskId}/comments/{commentId}", model);
            res.EnsureSuccessStatusCode();
        }

        // ✅ Delete comment
        public async Task DeleteCommentAsync(int taskId, int commentId)
        {
            await EnsureTokenAsync();

            var res = await _http.DeleteAsync($"/api/tasks/{taskId}/comments/{commentId}");
            res.EnsureSuccessStatusCode();
        }
    }
}
