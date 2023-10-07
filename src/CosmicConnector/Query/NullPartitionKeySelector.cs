namespace CosmicConnector.Query;

public sealed class NullPartitionKeySelector : IPartitionKeySelector
{
    public static readonly IPartitionKeySelector Instance = new NullPartitionKeySelector();

    private NullPartitionKeySelector() { }

    public string GetPartitionKey(object entity) => "";
}
