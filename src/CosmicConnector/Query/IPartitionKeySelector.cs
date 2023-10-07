namespace CosmicConnector.Query;

public interface IPartitionKeySelector
{
    string GetPartitionKey(object entity);
}
