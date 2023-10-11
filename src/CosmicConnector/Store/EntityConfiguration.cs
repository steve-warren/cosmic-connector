using CosmoDust.Query;

namespace CosmoDust;

public record EntityConfiguration(Type EntityType, string ContainerName, IStringSelector IdSelector, IStringSelector PartitionKeySelector);
