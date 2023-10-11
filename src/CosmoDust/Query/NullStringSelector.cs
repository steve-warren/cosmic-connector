namespace CosmoDust.Query;

internal sealed class NullStringSelector : IStringSelector
{
    public static readonly IStringSelector Instance = new NullStringSelector();

    private NullStringSelector() { }

    public string GetString(object entity) => "";
}
