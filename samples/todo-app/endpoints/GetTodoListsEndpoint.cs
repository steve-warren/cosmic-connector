using Cosmodust.Cosmos;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class GetTodoListsEndpoint : ControllerBase
{
    [HttpGet("api/todo/{ownerId}/lists/")]
    public async Task Get(
        string ownerId,
        [FromServices] QueryFacade queryFacade)
    {
        Response.ContentType = "application/json; charset=utf-8";

        await queryFacade.ExecuteQueryAsync(
            pipeWriter: Response.BodyWriter,
            containerName: "todo",
            partitionKey: ownerId,
            sql: @"select * from c where c.__type = 'TodoList' and c.ownerId = @ownerId",
            parameters: new { ownerId = ownerId });
    }
}
