using System.Diagnostics;

namespace CosmoDust.Cosmos.Memory;

public sealed class DefaultMemoryStreamProvider : IMemoryStreamProvider
{
    public static readonly DefaultMemoryStreamProvider Instance = new();

    private DefaultMemoryStreamProvider() { }

    public MemoryStream GetMemoryStream(string tag = "")
    {
        Debug.WriteLine("MemoryStream requested", tag);
        return new MemoryStream();
    }
}
