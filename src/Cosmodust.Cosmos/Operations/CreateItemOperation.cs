using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

internal class CreateItemOperation : ICosmosWriteOperation
{
    private static readonly ItemRequestOptions s_defaultItemRequestOptions = new ItemRequestOptions
        { EnableContentResponseOnWrite = false };

    private readonly Container _container;
    private readonly object _entity;

    public CreateItemOperation(Container container, object entity)
    {
        _container = container;
        _entity = entity;
    }

    public Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return _container.CreateItemAsync(
            _entity,
            requestOptions: s_defaultItemRequestOptions,
            cancellationToken: cancellationToken);
    }
}
