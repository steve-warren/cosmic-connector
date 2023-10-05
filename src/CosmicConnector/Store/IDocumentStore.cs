namespace CosmicConnector;

public interface IDocumentStore
{
    IDocumentSession CreateSession();
}
