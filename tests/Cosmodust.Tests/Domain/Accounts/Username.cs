namespace Cosmodust.Tests.Domain.Accounts;

public record Username(string Value)
{
    public override string ToString() =>
        Value;

    public static implicit operator Username(string username) =>
        new(username);

    public static implicit operator string(Username username) =>
        username.Value;
}
