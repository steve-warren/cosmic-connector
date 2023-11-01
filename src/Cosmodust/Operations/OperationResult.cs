using System.Net;

namespace Cosmodust.Operations;

public record OperationResult
{
    public required Type EntityType { get; init; }
    public required object? Entity { get; init; }
    public required HttpStatusCode StatusCode { get; init; }
    public string? ETag { get; set; }
    public double Cost { get; init; }
}
