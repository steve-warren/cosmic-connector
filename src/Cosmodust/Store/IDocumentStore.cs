using Cosmodust.Session;

namespace Cosmodust.Store;

public interface IDocumentStore
{
    IDocumentSession CreateSession();
}
