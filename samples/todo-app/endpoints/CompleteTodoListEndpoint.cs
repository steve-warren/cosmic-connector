using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class CompleteTodoListEndpoint : ControllerBase
{
    [HttpPut("api/lists/{id}/state/completed")]
    public Task<IActionResult> ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}
