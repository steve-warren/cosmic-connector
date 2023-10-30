namespace Cosmodust.Memory;

/// <summary>
/// Provides a way to get a <see cref="MemoryStream"/> instance.
/// </summary>
public interface IMemoryStreamProvider
{
    /// <summary>
    /// Gets a <see cref="MemoryStream"/> instance with an optional tag.
    /// </summary>
    /// <param name="tag">An optional tag to associate with the <see cref="MemoryStream"/>.</param>
    /// <returns>A <see cref="MemoryStream"/> instance.</returns>
    MemoryStream GetMemoryStream(string tag = "");
}
