using Microsoft.Azure.Cosmos;

namespace Cosmodust;

internal sealed class ContainerProvider
{
    private readonly Database _database;
    private readonly Dictionary<string, Container> _containers = new();

    public ContainerProvider(
        Database database)
    {
        _database = database;
    }

    public Container GetOrAddContainer(string containerName)
    {
        if (_containers.TryGetValue(containerName, out var container))
            return container;

        container = _database.GetContainer(containerName);

        _containers.Add(key: containerName, value: container);

        return container;
    }
}
