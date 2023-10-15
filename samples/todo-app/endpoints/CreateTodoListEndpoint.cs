using Cosmodust.Samples.TodoApp.Domain;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
[Route("api/todo/lists")]
public class CreateTodoListEndpoint : ControllerBase
{
    public record CreateTodoListRequest(string Name, string OwnerId);

    [HttpPost]
    public IActionResult CreateTodoList([FromBody] CreateTodoListRequest request)
    {
        var list = new TodoList(request.Name, Ksuid.NewKsuid("l_"), request.OwnerId);

        return Ok(list);
    }
}
