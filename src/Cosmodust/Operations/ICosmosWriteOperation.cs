using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

internal interface ICosmosWriteOperation
{
    Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default);
}
