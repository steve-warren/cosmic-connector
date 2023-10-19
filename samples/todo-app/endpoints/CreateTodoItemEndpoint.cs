using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
[Route("api/todo/lists/{listId}/items")]
public class CreateTodoItemEndpoint : ControllerBase
{
    public record CreateTodoItemRequest(
        string Name,
        [FromRoute]
        string ListId,
        string Priority,
        string Notes,
        string OwnerId,
        DateTimeOffset? Reminder);

    [HttpPost]
    public async Task<IActionResult> CreateTodoItem(
        [FromServices] IDocumentSession session,
        [FromBody] CreateTodoItemRequest request)
    {
        var list = await session.FindAsync<TodoList>(request.ListId, partitionKey: request.OwnerId);

        if (list is null)
            return NotFound();

        var item = new TodoItem(name: request.Name,
                                listId: list.Id,
                                ownerId: list.OwnerId,
                                priority: TodoItemPriority.Parse(request.Priority),
                                id: Ksuid.NewKsuid("i_"),
                                notes: request.Notes,
                                reminder: request.Reminder);

        list.AddItem(item);

        session.Update(list);
        session.Store(item);

        await session.CommitTransactionAsync();

        return Ok(item);
    }
}
