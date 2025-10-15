using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;
[Authorize]
[Route("Search")]
public class SearchController : Controller
{
    private readonly ITodoTaskWebApiService taskService;
    private readonly IUsersAuthWebApiService authService;

    public SearchController(ITodoTaskWebApiService taskService,IUsersAuthWebApiService authService)
    {
        this.taskService = taskService;
        this.authService = authService;
    }

    // GET: /Search
    public IActionResult Index()
    {
        if (string.IsNullOrEmpty(authService.JwtToken))
        {
            return this.RedirectToAction("Login", "Auth");
        }

        return this.View(new SearchViewModel());
    }

    // POST: /Search/Results
    [HttpPost]
    public async Task<IActionResult> Results(SearchViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View("Index", model);
        }

        var tasks = await taskService.SearchTasksAsync(
            model.Query,
            model.Status,
            model.DueBefore,
            model.AssignedUserId
        );

        model.Results = tasks.ToList();
        return this.View("Index", model);
    }
}
