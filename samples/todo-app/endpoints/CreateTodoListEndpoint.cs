using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class CreateTodoListEndpoint : ControllerBase
{
    public record CreateTodoListRequest(string Name, string OwnerId);

    [HttpPost("api/todo/lists")]
    public async Task<IActionResult> CreateTodoList(
        [FromServices] IDocumentSession session,
        [FromBody] CreateTodoListRequest request)
    {
        var account = await session.FindAsync<Account>(request.OwnerId);

        if (account is null)
            return NotFound();

        account.AddList();
        var list = new TodoList(name: request.Name,
                                id: Ksuid.NewKsuid("l_"),
                                ownerId: account.Id);

        session.Update(account);
        session.Store(list);

        await session.CommitTransactionAsync();

        return Ok(list);
    }
}
