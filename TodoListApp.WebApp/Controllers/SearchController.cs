using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;
[Route("Search")]
public class SearchController : Controller
{
    private readonly ITodoTaskWebApiService taskService;

    public SearchController(ITodoTaskWebApiService taskService)
    {
        this.taskService = taskService;
    }

    // GET: /Search
    public IActionResult Index()
    {
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

        var tasks = await this.taskService.SearchTasksAsync(
            model.Query,
            model.Status,
            model.DueBefore,
            model.AssignedUserId
        );

        model.Results = tasks.ToList();
        return this.View("Index", model); // We can render the results on the same page
    }
}
