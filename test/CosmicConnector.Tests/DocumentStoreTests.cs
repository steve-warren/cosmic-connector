namespace CosmicConnector.Tests;

public class DocumentStoreTests
{
    [Fact]
    public void Can_Create_Session()
    {
        var documentStore = new DocumentStore();

        var session = documentStore.CreateSession();

        session.Should().NotBeNull(because: "we should be able to create a session");
    }
}
