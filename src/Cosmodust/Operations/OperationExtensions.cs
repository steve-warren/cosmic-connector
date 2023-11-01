using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

internal static class OperationExtensions
{
    public static OperationResult ToOperationResult<TEntity>(
        this ItemResponse<TEntity> response)
    {
        return new OperationResult()
        {
            EntityType = typeof(TEntity),
            Entity = response.Resource,
            StatusCode = response.StatusCode,
            Cost = response.RequestCharge,
            ETag = response.ETag
        };
    }
}
