using CosmoDust.Linq;

namespace CosmoDust.Cosmos.Tests.Domain.Accounts;

public class AccountPlanRepository
{
    private readonly IDocumentSession _session;

    public AccountPlanRepository(IDocumentSession session)
    {
        _session = session;
    }

    public ValueTask<AccountPlan?> FindByIdAsync(string id)
    {
        return _session.FindAsync<AccountPlan>(id);
    }

    public Task<List<AccountPlan>> FindByNameAsync(string name)
    {
        return _session.Query<AccountPlan>().Where(e => e.Name == name).Take(10).ToListAsync();
    }

    public Task<List<AccountPlan>> AllAsync()
    {
        return _session.Query<AccountPlan>().ToListAsync();
    }
}
