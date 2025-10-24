using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities;
using TodoListApp.WebApi.Models; // Add this

namespace TodoListApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class TodoUsersController : ControllerBase
    {
        private readonly UsersDbContext _usersDb;

        public TodoUsersController(UsersDbContext usersDb)
        {
            _usersDb = usersDb;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _usersDb.Users.ToListAsync();

            // Return TodoUserModel with UserName included
            var userModels = users.Select(u => new TodoUserModel
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName // Add this line
            }).ToList();

            return Ok(userModels);
        }
    }
}
