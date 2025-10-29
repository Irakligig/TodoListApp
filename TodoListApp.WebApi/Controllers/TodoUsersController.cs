using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database;
using TodoListApp.WebApi.Models;

namespace TodoListApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class TodoUsersController : ControllerBase
    {
        private readonly UsersDbContext usersDb;

        public TodoUsersController(UsersDbContext usersDb)
        {
            this.usersDb = usersDb;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await this.usersDb.Users.ToListAsync();

            var userModels = users.Select(u => new TodoUserModel
            {
                Id = u.Id,
                FullName = u.FullName,
                UserName = u.UserName,
            }).ToList();

            return this.Ok(userModels);
        }
    }
}
