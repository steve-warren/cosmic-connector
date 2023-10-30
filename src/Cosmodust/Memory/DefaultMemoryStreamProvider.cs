using System.Diagnostics;

namespace Cosmodust.Memory;

/// <summary>
/// Provides a default implementation of the <see cref="IMemoryStreamProvider"/> interface.
/// </summary>
public sealed class DefaultMemoryStreamProvider : IMemoryStreamProvider
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IMemoryStreamProvider"/> interface using a <see cref="MemoryStream"/>.
    /// </summary>
    public static readonly DefaultMemoryStreamProvider Instance = new();

    private DefaultMemoryStreamProvider() { }

    /// <summary>
    /// Returns a new <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="tag">An optional tag to associate with the request.</param>
    /// <returns>A new <see cref="MemoryStream"/> instance.</returns>
    public MemoryStream GetMemoryStream(string tag = "")
    {
        Debug.WriteLine("MemoryStream requested", tag);
        return new MemoryStream();
    }
}
