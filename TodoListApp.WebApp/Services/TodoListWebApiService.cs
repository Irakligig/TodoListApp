using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApp.Services
{
    public class TodoListWebApiService : ITodoListWebApiService
    {
        private readonly HttpClient _http;
        private string? _jwtToken;

        public TodoListWebApiService(HttpClient http)
        {
            _http = http;
        }

        // Fetch token from WebAPI if not set
        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_jwtToken))
            {
                return; // token already set
            }

            var tokenResponse = await _http.GetFromJsonAsync<TokenResponse>("/api/token");
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.token))
            {
                throw new Exception("Failed to retrieve JWT token from WebAPI.");
            }

            _jwtToken = tokenResponse.token;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
        }

        public async Task<IEnumerable<TodoListModel>> GetTodoListsAsync()
        {
            await EnsureTokenAsync();

            // Log headers
            Console.WriteLine("Request headers:");
            foreach (var header in _http.DefaultRequestHeaders)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            var res = await _http.GetAsync("/api/todolist");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoListModel>>()
                   ?? Array.Empty<TodoListModel>();
        }

        public async Task AddTodoListAsync(TodoListModel newList)
        {
            await EnsureTokenAsync();

            try
            {
                var response = await _http.PostAsJsonAsync("/api/todolist", newList);

                if (response.IsSuccessStatusCode)
                {
                    return; // Success
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request failed: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateTodoListAsync(TodoListModel updatedList)
        {
            await EnsureTokenAsync();
            var res = await _http.PutAsJsonAsync($"/api/todolist/{updatedList.Id}", updatedList);
            res.EnsureSuccessStatusCode();
        }

        public async Task DeleteTodoListAsync(int id)
        {
            await EnsureTokenAsync();
            var res = await _http.DeleteAsync($"/api/todolist/{id}");
            res.EnsureSuccessStatusCode();
        }
    }

    // DTO to match /api/token response
    public class TokenResponse
    {
        public string token { get; set; } = string.Empty;
    }
}
