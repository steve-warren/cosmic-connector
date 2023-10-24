using Cosmodust.Query;
using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class DeleteTodoItemsEndpoint : ControllerBase
{
    [HttpDelete("api/accounts/{ownerId}/lists/{listId}/items")]
    public async Task<IActionResult> Delete(
        [FromServices] IDocumentSession session,
        string ownerId,
        string listId)
    {
        var list = await session.FindAsync<TodoList>(
            id: listId,
            partitionKey: ownerId);

        if (list is null)
            return NotFound();

        var query = session.Query<TodoItem>(
            "select * from c where c.__type = 'TodoItem'",
            partitionKey: ownerId);

        await foreach (var item in query.ToAsyncEnumerable())
        {
            list.RemoveItem(item);
            session.Remove(item);
        }

        session.Update(list);

        await session.CommitTransactionAsync();

        return Ok();
    }
}
