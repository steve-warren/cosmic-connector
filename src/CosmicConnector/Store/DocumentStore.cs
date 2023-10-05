using Microsoft.Azure.Cosmos;

namespace CosmicConnector;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore()
    {

    }

    internal EntityIdAccessor IdAccessor { get; } = new();

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this);
    }

    public DocumentStore ConfigureEntity<TEntity>() where TEntity : class
    {
        IdAccessor.RegisterEntity<TEntity>();

        return this;
    }
}
