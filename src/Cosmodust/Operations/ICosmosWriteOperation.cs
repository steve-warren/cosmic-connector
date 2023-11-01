using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal interface ICosmosWriteOperation
{
    Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
