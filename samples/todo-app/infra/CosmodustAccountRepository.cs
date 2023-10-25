using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;

namespace Cosmodust.Samples.TodoApp.Infra;

public class CosmodustAccountRepository : IAccountRepository
{
    private readonly DocumentSession _session;

    public CosmodustAccountRepository(DocumentSession session)
    {
        _session = session;
    }

    public ValueTask<Account?> FindAsync(string id) =>
        _session.FindAsync<Account>(id: id, partitionKey: id);

    public void Update(Account account) =>
        _session.Update(account);
}
