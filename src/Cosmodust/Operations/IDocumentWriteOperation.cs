using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal interface IDocumentWriteOperation
{
    Task<OperationResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
