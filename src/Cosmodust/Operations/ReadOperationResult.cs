using System.Net;

namespace Cosmodust.Operations;

public record ReadOperationResult<TEntity>(
    TEntity? Entity,
    HttpStatusCode StatusCode,
    string ETag = "",
    double Cost = 0);
