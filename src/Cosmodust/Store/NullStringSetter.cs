namespace Cosmodust.Store;

internal sealed class NullStringSetter : IStringSetter
{
    public static readonly NullStringSetter Instance = new();

    private NullStringSetter() { }

    public void SetString(object entity, object? value)
    {
    }
}
