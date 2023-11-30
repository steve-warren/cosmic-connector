namespace Cosmodust.Store;

internal sealed class StringValueSelector : IStringSelector
{

    public StringValueSelector(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public string GetString(object entity) => Value;
}
