using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
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

    [HttpPost("api/todo/lists/{listId}/items")]
    public async Task<IActionResult> CreateTodoItem(
        [FromServices] ITodoListRepository todoLists,
        [FromServices] ITodoItemRepository todoItems,
        [FromServices] IUnitOfWork unitOfWork,
        [FromBody] CreateTodoItemRequest request)
    {
        var list = await todoLists.FindAsync(request.OwnerId, request.ListId);

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
        todoItems.Add(item);

        todoLists.Update(list);

        await unitOfWork.SaveChangesAsTransactionAsync();

        return Ok(item);
    }
}
