using Microsoft.Azure.Cosmos;

namespace CosmicConnector;

public sealed class DocumentStore : IDocumentStore
{
    public DocumentStore()
    {

    }

    public IDocumentSession CreateSession()
    {
        return new DocumentSession(this);
    }
}
