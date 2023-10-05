namespace CosmicConnector;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore()
    {

    }

    internal IdentityAccessor IdAccessor { get; } = new();

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(IdAccessor);
    }

    public DocumentStore ConfigureEntity<TEntity>() where TEntity : class
    {
        IdAccessor.Register<TEntity>();

        return this;
    }
}
