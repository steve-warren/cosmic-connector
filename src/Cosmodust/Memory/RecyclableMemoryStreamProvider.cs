using System.Diagnostics;
using Microsoft.IO;

namespace Cosmodust.Memory;

public class RecyclableMemoryStreamProvider : IMemoryStreamProvider
{
    private readonly RecyclableMemoryStreamManager _pool = new();

    public MemoryStream GetMemoryStream(string tag = "") =>
        _pool.GetStream(Guid.NewGuid(), tag);
}
