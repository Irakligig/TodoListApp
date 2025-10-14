using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.Services.Database.Entities; // 👈 Make sure this is included!

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

            return Ok(users.Select(u => new
            {
                Id = u.Id,
                FullName = u.FullName// ✅ Explicit naming avoids ambiguity
            }));
        }
    }
}
