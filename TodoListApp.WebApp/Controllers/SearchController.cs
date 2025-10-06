using Microsoft.AspNetCore.Mvc;
using TodoListApp.WebApi.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp.Controllers;
[Route("Search")]
public class SearchController : Controller
{
    private readonly ITodoTaskWebApiService _taskService;

    public SearchController(ITodoTaskWebApiService taskService)
    {
        _taskService = taskService;
    }

    // GET: /Search
    public IActionResult Index()
    {
        return View(new SearchViewModel());
    }

    // POST: /Search/Results
    [HttpPost]
    public async Task<IActionResult> Results(SearchViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var tasks = await _taskService.SearchTasksAsync(
            model.Query,
            model.Status,
            model.DueBefore,
            model.AssignedUserId
        );

        model.Results = tasks.ToList();
        return View("Index", model); // We can render the results on the same page
    }
}
