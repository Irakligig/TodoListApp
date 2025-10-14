using System.Net.Http;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoCommentWebApiService : ITodoCommentWebApiService
    {
        private readonly HttpClient _httpClient;

        public TodoCommentWebApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TodoCommentModel>> GetCommentsAsync(int taskId)
        {
            var comments = await _httpClient.GetFromJsonAsync<List<TodoCommentModel>>($"api/tasks/{taskId}/comments");
            return comments ?? new List<TodoCommentModel>();
        }

        public async Task AddCommentAsync(int taskId, string text)
        {
            var model = new TodoCommentCreateModel { Text = text };
            var response = await _httpClient.PostAsJsonAsync($"api/tasks/{taskId}/comments", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task EditCommentAsync(int taskId, int commentId, string newText)
        {
            var model = new TodoCommentEditModel { Text = newText };
            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}/comments/{commentId}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCommentAsync(int taskId, int commentId)
        {
            var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}/comments/{commentId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
