using Cosmodust.Samples.TodoApp.Domain;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class CreateTodoListEndpoint : ControllerBase
{
    public record CreateTodoListRequest(string Name, string OwnerId);

    [HttpPost("api/todo/lists")]
    public async Task<IActionResult> CreateTodoList(
        [FromServices] IUnitOfWork unitOfWork,
        [FromServices] IAccountRepository accounts,
        [FromServices] ITodoListRepository todoLists,
        [FromBody] CreateTodoListRequest request)
    {
        var account = await accounts.FindAsync(id: request.OwnerId);

        if (account is null)
            return NotFound();

        account.AddList();
        accounts.Update(account);

        var list = new TodoList(name: request.Name,
                                id: Ksuid.NewKsuid("l_"),
                                ownerId: account.Id);

        todoLists.Add(list);

        await unitOfWork.SaveChangesAsTransactionAsync();

        return Ok(list);
    }
}
