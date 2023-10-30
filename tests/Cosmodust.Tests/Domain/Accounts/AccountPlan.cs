using System.Linq.Expressions;

namespace Cosmodust.Cosmos.Tests.Domain.Accounts;

public class AccountPlan
{
    public AccountPlan(string id)
    {
        Id = id;
    }

    public string Id { get; set; } = "";
    public string Name { get; set; } = "Test Plan";
}

public static class AccountPlanSpecifications
{
    /// <summary>
    /// Returns an expression that filters account plans by their ID.
    /// </summary>
    public static Expression<Func<AccountPlan?, bool>> ById => e => e.Id == "test";
    public static Expression<Func<AccountPlan?, bool>> ByName => e => e.Name == "Test Plan";
}
