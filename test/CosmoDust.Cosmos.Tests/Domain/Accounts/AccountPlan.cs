namespace CosmoDust.Cosmos.Tests.Domain.Accounts;

public class AccountPlan
{
    public AccountPlan(string id)
    {
        Id = id;
    }

    public string Id { get; set; } = "";
    public string Name { get; set; } = "Test Plan";
}
