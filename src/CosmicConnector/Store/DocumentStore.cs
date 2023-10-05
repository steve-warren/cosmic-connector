namespace CosmicConnector;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore(IDatabaseFacade databaseFacade)
    {
        DatabaseFacade = databaseFacade;
    }

    internal IdentityAccessor IdAccessor { get; } = new();
    public IDatabaseFacade DatabaseFacade { get; }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(IdAccessor, DatabaseFacade);
    }

    public DocumentStore ConfigureEntity<TEntity>() where TEntity : class
    {
        IdAccessor.Register<TEntity>();

        return this;
    }
}
