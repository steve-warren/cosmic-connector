using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal interface ICosmosWriteOperation
{
    Task<ItemResponse<object>> ExecuteAsync(CancellationToken cancellationToken = default);
}
