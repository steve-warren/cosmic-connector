namespace CosmoDust;

public interface IDocumentStore
{
    IDocumentSession CreateSession();
}
