using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoTaskWebApiService : ITodoTaskWebApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor using IHttpClientFactory
        public TodoTaskWebApiService(IHttpClientFactory httpFactory, IHttpContextAccessor httpContextAccessor)
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
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }


        public async Task<IEnumerable<TodoTaskModel>> GetTasksAsync(int todoListId)
        {
            AddAuthHeader();
            var res = await _http.GetAsync($"api/todolists/{todoListId}/tasks");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>()
                   ?? Array.Empty<TodoTaskModel>();
        }

        public async Task<TodoTaskModel?> GetByIdAsync(int todoListId, int taskId)
        {
            AddAuthHeader();
            var res = await _http.GetAsync($"api/todolists/{todoListId}/tasks/{taskId}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TodoTaskModel>();
        }

        public async Task<TodoTaskModel?> CreateAsync(int todoListId, TodoTaskModel model)
        {
            AddAuthHeader();
            var res = await _http.PostAsJsonAsync($"api/todolists/{todoListId}/tasks", model);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TodoTaskModel>();
        }

        public async Task UpdateAsync(int todoListId, int taskId, TodoTaskModel model)
        {
            AddAuthHeader();
            var res = await _http.PutAsJsonAsync($"api/todolists/{todoListId}/tasks/{taskId}", model);
            res.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int todoListId, int taskId)
        {
            AddAuthHeader();
            var res = await _http.DeleteAsync($"api/todolists/{todoListId}/tasks/{taskId}");
            res.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<TodoTaskModel>> GetAssignedAsync(string? status = null, string? sortBy = null)
        {
            AddAuthHeader();

            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query.Add($"status={Uri.EscapeDataString(status)}");
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query.Add($"sortby={Uri.EscapeDataString(sortBy.ToLowerInvariant())}");
            }

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
            var res = await _http.GetAsync($"api/tasks/assigned{queryString}");
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoTaskModel>>()
                   ?? Array.Empty<TodoTaskModel>();
        }

        public async Task UpdateStatusAsync(int taskId, bool status)
        {
            AddAuthHeader();
            var res = await _http.PatchAsync($"api/tasks/assigned/{taskId}/status?isCompleted={status}", null);
            res.EnsureSuccessStatusCode();
        }

        public async Task ReassignTaskAsync(int taskId, string newUserId)
        {
            AddAuthHeader();
            var dto = new ReassignTaskDto { NewUserId = newUserId };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/tasks/assigned/{taskId}/assign")
            {
                Content = JsonContent.Create(dto)
            };
            var res = await _http.SendAsync(request);
            res.EnsureSuccessStatusCode();
        }

        public async Task<List<TodoUserModel>> GetAllUsersAsync()
        {
            AddAuthHeader();
            var res = await _http.GetAsync("api/users");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<TodoUserModel>>() ?? new List<TodoUserModel>();
        }

        public async Task<IEnumerable<TodoTaskModel>> SearchTasksAsync(
    string? query = null, bool? status = null, DateTime? dueBefore = null, string? assignedUserId = null)
        {
            AddAuthHeader();

            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                queryParams.Add($"query={Uri.EscapeDataString(query)}");
            }

            if (status.HasValue)
            {
                queryParams.Add($"status={status.Value}");
            }

            if (dueBefore.HasValue)
            {
                queryParams.Add($"dueBefore={dueBefore.Value:yyyy-MM-dd}");
            }

            if (!string.IsNullOrWhiteSpace(assignedUserId))
            {
                queryParams.Add($"assignedUserId={Uri.EscapeDataString(assignedUserId)}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var url = $"/api/tasks/search{queryString}";

            var tasks = await _http.GetFromJsonAsync<List<TodoTaskModel>>(url);
            return tasks ?? new List<TodoTaskModel>();
        }

    }
}
