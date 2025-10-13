using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Services;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("api/users")]
public class TodoUsersController : ControllerBase
{
    private readonly IUsersDatabaseService usersService;

    public TodoUsersController(IUsersDatabaseService usersService)
    {
        this.usersService = usersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await this.usersService.GetAllAsync();
        return this.Ok(users.Select(u => new { u.Id, u.FullName }));
    }
}
