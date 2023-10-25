using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class DeleteTodoItemEndpoint : ControllerBase
{
    [HttpDelete("api/accounts/{ownerId}/lists/{listId}/items/{itemId}")]
    public async Task<IActionResult> DeleteTodoItem(
        [FromServices] IDocumentSession session,
        string ownerId,
        string listId,
        string itemId)
    {
        var list = await session.FindAsync<TodoList>(id: listId, partitionKey: ownerId);
        var item = await session.FindAsync<TodoItem>(id: itemId, partitionKey: ownerId);
        
        if (list is null || item is null)
            return NotFound();

        list.RemoveItem(item);

        session.Update(list);
        session.Remove(item);

        await session.CommitTransactionAsync();

        return Ok(item);
    }
}
