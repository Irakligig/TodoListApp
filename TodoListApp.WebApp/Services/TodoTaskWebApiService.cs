using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoTaskWebApiService : ITodoTaskWebApiService
    {
        private readonly HttpClient http;
        private string? _jwtToken;

        public TodoTaskWebApiService(HttpClient http)
        {
            this.http = http;
        }

        // Ensure JWT is retrieved & attached
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

        // ======================
        // Tasks inside a TodoList
        // ======================
        public async Task<IEnumerable<TodoTaskModel>> GetTasksAsync(int todoListId)
        {
            await EnsureTokenAsync();

            var res = await http.GetAsync($"/api/todolists/{todoListId}/tasks");

            if (res.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new System.Security.SecurityException($"Access denied to Todo List ID {todoListId}.");
            }

            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Todo List with ID {todoListId} not found.");
            }

            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>()
                   ?? Array.Empty<TodoTaskModel>();
        }

        public async Task<TodoTaskModel?> GetByIdAsync(int todoListId, int taskId)
        {
            await EnsureTokenAsync();

            var res = await http.GetAsync($"/api/todolists/{todoListId}/tasks/{taskId}");
            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TodoTaskModel>();
        }

        public async Task<TodoTaskModel?> CreateAsync(int todoListId, TodoTaskModel model)
        {
            await EnsureTokenAsync();

            var res = await http.PostAsJsonAsync($"/api/todolists/{todoListId}/tasks", model);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Create failed: {res.StatusCode}");
            }

            return await res.Content.ReadFromJsonAsync<TodoTaskModel>();
        }

        public async Task UpdateAsync(int todoListId, int taskId, TodoTaskModel model)
        {
            await EnsureTokenAsync();

            var res = await http.PutAsJsonAsync($"/api/todolists/{todoListId}/tasks/{taskId}", model);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Update failed: {res.StatusCode}");
            }
        }

        public async Task DeleteAsync(int todoListId, int taskId)
        {
            await EnsureTokenAsync();

            var res = await http.DeleteAsync($"/api/todolists/{todoListId}/tasks/{taskId}");
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Delete failed: {res.StatusCode}");
            }
        }

        // ======================
        // Assigned tasks
        // ======================
        public async Task<IEnumerable<TodoTaskModel>> GetAssignedAsync(string? status = null, string? sortBy = null)
        {
            await EnsureTokenAsync();

            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                q.Add($"status={Uri.EscapeDataString(status)}");
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // Change to lowercase "sortby" to match API parameter
                q.Add($"sortby={Uri.EscapeDataString(sortBy.ToLower())}"); // Also convert value to lowercase
            }

            var qs = q.Count > 0 ? "?" + string.Join("&", q) : string.Empty;
            var res = await http.GetAsync($"/api/tasks/assigned{qs}");

            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                return Array.Empty<TodoTaskModel>();
            }

            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>() ?? Array.Empty<TodoTaskModel>();
        }

        public async Task UpdateStatusAsync(int taskId, bool status)
        {
            await EnsureTokenAsync();

            var res = await http.PatchAsync($"/api/tasks/assigned/{taskId}/status?isCompleted={status}", null);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Update status failed: {res.StatusCode}");
            }
        }

        public async Task ReassignTaskAsync(int taskId, string newUserId)
        {
            await EnsureTokenAsync();

            var dto = new ReassignTaskDto { NewUserId = newUserId };

            var request = new HttpRequestMessage(HttpMethod.Patch,
                $"/api/tasks/assigned/{taskId}/assign")
            {
                Content = JsonContent.Create(dto)
            };

            var res = await http.SendAsync(request);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync(); // get body from API
                throw new HttpRequestException($"Reassign failed: {res.StatusCode}, Details: {error}");
            }
        }

        public async Task<List<TodoUserModel>> GetAllUsersAsync()
        {
            await EnsureTokenAsync();

            var res = await http.GetAsync("/api/users");

            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch users: {res.StatusCode}");
            }

            // Deserialize into a list of UserModel
            var users = await res.Content.ReadFromJsonAsync<List<TodoUserModel>>();

            return users ?? new List<TodoUserModel>();
        }

    }
}
