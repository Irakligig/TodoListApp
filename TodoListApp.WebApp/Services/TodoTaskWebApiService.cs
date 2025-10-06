using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoTaskWebApiService : ITodoTaskWebApiService
    {
        private readonly HttpClient http;
        private string? jwtToken;

        public TodoTaskWebApiService(HttpClient http)
        {
            this.http = http;
        }

        // Ensure JWT is retrieved & attached
        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(this.jwtToken))
            {
                return;
            }

            var tokenResponse = await this.http.GetFromJsonAsync<TokenResponse>("/api/token").ConfigureAwait(false);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.token))
            {
                throw new InvalidOperationException("Failed to retrieve JWT token from WebAPI.");
            }

            this.jwtToken = tokenResponse.token;
            this.http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.jwtToken);
        }

        // ======================
        // Tasks inside a TodoList
        // ======================
        public async Task<IEnumerable<TodoTaskModel>> GetTasksAsync(int todoListId)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.GetAsync($"/api/todolists/{todoListId}/tasks").ConfigureAwait(false);

            if (res.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new System.Security.SecurityException($"Access denied to Todo List ID {todoListId}.");
            }

            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Todo List with ID {todoListId} not found.");
            }

            _ = res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>().ConfigureAwait(false)
                   ?? Array.Empty<TodoTaskModel>();
        }

        public async Task<TodoTaskModel?> GetByIdAsync(int todoListId, int taskId)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.GetAsync($"/api/todolists/{todoListId}/tasks/{taskId}").ConfigureAwait(false);
            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            _ = res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TodoTaskModel>().ConfigureAwait(false);
        }

        public async Task<TodoTaskModel?> CreateAsync(int todoListId, TodoTaskModel model)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.PostAsJsonAsync($"/api/todolists/{todoListId}/tasks", model).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Create failed: {res.StatusCode}");
            }

            return await res.Content.ReadFromJsonAsync<TodoTaskModel>().ConfigureAwait(false);
        }

        public async Task UpdateAsync(int todoListId, int taskId, TodoTaskModel model)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.PutAsJsonAsync($"/api/todolists/{todoListId}/tasks/{taskId}", model).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Update failed: {res.StatusCode}");
            }
        }

        public async Task DeleteAsync(int todoListId, int taskId)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.DeleteAsync($"/api/todolists/{todoListId}/tasks/{taskId}").ConfigureAwait(false);
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
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query.Add($"status={Uri.EscapeDataString(status)}");
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // Change to lowercase "sortby" to match API parameter
                query.Add($"sortby={Uri.EscapeDataString(sortBy.ToLowerInvariant())}"); // Also convert value to lowercase
            }

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
            var res = await this.http.GetAsync($"/api/tasks/assigned{queryString}").ConfigureAwait(false);

            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                return Array.Empty<TodoTaskModel>();
            }

            _ = res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>().ConfigureAwait(false)
                ?? Array.Empty<TodoTaskModel>();
        }

        public async Task UpdateStatusAsync(int taskId, bool status)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.PatchAsync($"/api/tasks/assigned/{taskId}/status?isCompleted={status}", null).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Update status failed: {res.StatusCode}");
            }
        }

        public async Task ReassignTaskAsync(int taskId, string newUserId)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var dto = new ReassignTaskDto { NewUserId = newUserId };

            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/tasks/assigned/{taskId}/assign")
            {
                Content = JsonContent.Create(dto),
            };

            var res = await this.http.SendAsync(request).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync().ConfigureAwait(false); // get body from API
                throw new HttpRequestException($"Reassign failed: {res.StatusCode}, Details: {error}");
            }
        }

        public async Task<List<TodoUserModel>> GetAllUsersAsync()
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var res = await this.http.GetAsync("/api/users").ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch users: {res.StatusCode}");
            }

            // Deserialize into a list of UserModel
            var users = await res.Content.ReadFromJsonAsync<List<TodoUserModel>>().ConfigureAwait(false);

            return users ?? new List<TodoUserModel>();
        }

        public async Task<IEnumerable<TodoTaskModel>> SearchTasksAsync(
            string? query = null,
            bool? status = null,
            DateTime? dueBefore = null,
            string? assignedUserId = null)
        {
            await this.EnsureTokenAsync().ConfigureAwait(false);

            var url = "/api/tasks/search?";

            if (!string.IsNullOrWhiteSpace(query))
            {
                url += $"query={Uri.EscapeDataString(query)}&";
            }

            if (status.HasValue)
            {
                url += $"status={status.Value}&";
            }

            if (dueBefore.HasValue)
            {
                url += $"dueBefore={dueBefore.Value:yyyy-MM-dd}&";
            }

            if (!string.IsNullOrWhiteSpace(assignedUserId))
            {
                url += $"assignedUserId={Uri.EscapeDataString(assignedUserId)}&";
            }

            // Remove trailing '&'
            url = url.TrimEnd('&');

            try
            {
                var tasks = await this.http.GetFromJsonAsync<List<TodoTaskModel>>(url).ConfigureAwait(false);
                return tasks ?? new List<TodoTaskModel>();
            }
            catch (HttpRequestException)
            {
                // Optionally log the error
                return new List<TodoTaskModel>();
            }
        }
    }
}
