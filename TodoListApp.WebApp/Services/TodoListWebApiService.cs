using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoListApp.WebApi.Models;
using Microsoft.AspNetCore.Http;

namespace TodoListApp.WebApp.Services
{
    public class TodoListWebApiService : ITodoListWebApiService
    {
        private readonly HttpClient _http;
        private readonly IUsersAuthWebApiService _authService;

        public TodoListWebApiService(IHttpClientFactory httpFactory, IUsersAuthWebApiService authService)
        {
            _http = httpFactory.CreateClient("WebApiClient");
            _authService = authService;
        }

        private void AttachJwt()
        {
            var token = _authService.JwtToken;
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IEnumerable<TodoListModel>> GetTodoListsAsync()
        {
            AttachJwt();
            var res = await _http.GetAsync("/api/todolist");
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<IEnumerable<TodoListModel>>()
                   ?? Array.Empty<TodoListModel>();
        }

        public async Task<bool> AddTodoListAsync(TodoListModel newList)
        {
            AttachJwt();
            var res = await _http.PostAsJsonAsync("/api/todolist", newList);
            res.EnsureSuccessStatusCode();
            return res.IsSuccessStatusCode; // Return true if request succeeded
        }

        public async Task<bool> UpdateTodoListAsync(TodoListModel updatedList)
        {
            AttachJwt();
            var res = await _http.PutAsJsonAsync($"/api/todolist/{updatedList.Id}", updatedList);
            res.EnsureSuccessStatusCode();
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTodoListAsync(int id)
        {
            AttachJwt();
            var res = await _http.DeleteAsync($"/api/todolist/{id}");
            res.EnsureSuccessStatusCode();
            return res.IsSuccessStatusCode;
        }
    }
}
