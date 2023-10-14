namespace Cosmodust;

public interface IDocumentStore
{
    IDocumentSession CreateSession();
}
