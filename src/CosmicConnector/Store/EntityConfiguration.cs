using CosmicConnector.Query;

namespace CosmicConnector;

public record EntityConfiguration(Type EntityType, string DatabaseName, string ContainerName, IStringSelector IdSelector, IStringSelector PartitionKeySelector);
