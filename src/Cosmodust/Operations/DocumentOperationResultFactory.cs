using System.Net;

namespace Cosmodust.Operations;

public static class DocumentOperationResultFactory
{
    public static IDocumentOperationResult Create(OperationResult operationResult)
    {
        return operationResult.StatusCode switch
        {
            HttpStatusCode.PreconditionFailed => new ConcurrencyConflictDocumentOperationResult(),
            _ => new SuccessDocumentOperationResult()
        };
    }
}
