using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;

namespace Cosmodust.Samples.TodoApp.Infra;

public class CosmodustUnitOfWork : IUnitOfWork
{
    private readonly DocumentSession _session;

    public CosmodustUnitOfWork(DocumentSession session)
    {
        _session = session;
    }

    public Task SaveChangesAsync() =>
        _session.CommitAsync();

    public Task SaveChangesAsTransactionAsync() =>
        _session.CommitTransactionAsync();
}
