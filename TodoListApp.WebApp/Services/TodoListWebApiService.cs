using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;
using Microsoft.AspNetCore.Http;

namespace TodoListApp.WebApp.Services
{
    public class TodoListWebApiService : ITodoListWebApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TodoListWebApiService(HttpClient http, IHttpContextAccessor httpContextAccessor)
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
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IEnumerable<TodoListModel>> GetTodoListsAsync()
        {
            AddAuthHeader();
            var response = await _http.GetAsync("api/todolist");

            if (!response.IsSuccessStatusCode)
            {
                return Array.Empty<TodoListModel>();
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<TodoListModel>>() ?? Array.Empty<TodoListModel>();
        }

        public async Task<bool> AddTodoListAsync(TodoListModel newList)
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("api/todolist", newList);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTodoListAsync(TodoListModel updatedList)
        {
            AddAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/todolist/{updatedList.Id}", updatedList);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTodoListAsync(int id)
        {
            AddAuthHeader();
            var response = await _http.DeleteAsync($"api/todolist/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
