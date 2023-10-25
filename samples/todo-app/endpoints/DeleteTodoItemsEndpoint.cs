using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class DeleteTodoItemsEndpoint : ControllerBase
{
    [HttpDelete("api/accounts/{ownerId}/lists/{listId}/items")]
    public async Task<IActionResult> Delete(
        [FromServices] ITodoListRepository todoLists,
        [FromServices] ITodoItemRepository todoItems,
        [FromServices] IUnitOfWork unitOfWork,
        string ownerId,
        string listId)
    {
        var list = await todoLists.FindAsync(ownerId, listId);

        if (list is null)
            return NotFound();

        var listItems = await todoItems.FindByListIdAsync(ownerId, listId);

        foreach (var item in listItems)
        {
            list.RemoveItem(item);
            todoItems.Remove(item);
        }

        todoLists.Update(list);

        await unitOfWork.SaveChangesAsTransactionAsync();

        return Ok();
    }
}
