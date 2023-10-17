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
    public async Task<IActionResult> CreateTodoList(
        [FromServices] IDocumentSession session,
        [FromBody] CreateTodoListRequest request)
    {
        var list = new TodoList(name: request.Name,
                                id: Ksuid.NewKsuid("l_"),
                                ownerId: request.OwnerId);

        session.Store(list);

        await session.CommitAsync();

        return Ok(list);
    }
}
