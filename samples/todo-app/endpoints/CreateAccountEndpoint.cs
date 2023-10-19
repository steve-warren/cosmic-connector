using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;
using KsuidDotNet;
using Microsoft.AspNetCore.Mvc;

namespace Cosmodust.Samples.TodoApp.Endpoints;

[ApiController]
public class CreateAccountEndpoint : ControllerBase
{
    public record CreateAccountRequest();

    [HttpPost("api/accounts")]
    public async Task<IActionResult> CreateTodoList(
        [FromServices] IDocumentSession session,
        [FromBody] CreateAccountRequest request)
    {
        var account = new Account(id: Ksuid.NewKsuid("a_"));

        session.Store(account);

        await session.CommitAsync();

        return Ok(account);
    }
}
