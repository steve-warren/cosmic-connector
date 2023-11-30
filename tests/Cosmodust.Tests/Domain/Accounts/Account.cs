namespace Cosmodust.Tests.Domain.Accounts;

public class Account
{
    private readonly HashSet<object> _domainEvents = new();
    public required string Username { get; init; }
    public required string Email { get; init; }

    public void RaiseEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
