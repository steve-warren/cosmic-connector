using System.Diagnostics;
using Cosmodust.Json;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Operations;

public class TransactionalBatchOperation
{
    private readonly Database _database;
    private readonly IEnumerable<EntityEntry> _entries;

    public TransactionalBatchOperation(
        Database database,
        IEnumerable<EntityEntry> entries)
    {
        _database = database;
        _entries = entries;
    }

    public async Task<IReadOnlyList<TransactionalBatchResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var containerAndPartitionKey = _entries
            .GroupBy(e => (e.ContainerName, e.PartitionKey));

        var responses = new List<TransactionalBatchResponse>();

        foreach (var entriesGrouping in containerAndPartitionKey)
        {
            var response = await ExecuteTransactionalBatchAsync(
                containerName: entriesGrouping.Key.ContainerName,
                partitionKey: entriesGrouping.Key.PartitionKey,
                entries: entriesGrouping,
                cancellationToken: cancellationToken);

            responses.Add(response);
        }

        return responses;
    }

    private async Task<TransactionalBatchResponse> ExecuteTransactionalBatchAsync(
        string containerName,
        string partitionKey,
        IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken = default)
    {
        var container = _database.GetContainer(containerName);

        var batch = container.CreateTransactionalBatch(new PartitionKey(partitionKey));
        var batchOptions = new TransactionalBatchItemRequestOptions
            { EnableContentResponseOnWrite = false };

        var entityEntries = entries.ToList();
        var domainEvents = new List<Dictionary<string, object>>();

        foreach (var entry in entityEntries)
        {
            // send the json properties to the provider
            // for the json serializer to pick up
            entry.WriteShadowProperties();

            _ = entry.State switch
            {
                EntityState.Added => batch.CreateItem(entry.Entity, batchOptions),
                EntityState.Removed => batch.DeleteItem(entry.Id, batchOptions),
                EntityState.Modified => batch.ReplaceItem(entry.Id, entry.Entity, batchOptions),
                EntityState.Unchanged or
                    EntityState.Detached => batch,
                _ => throw new InvalidOperationException()
            };

            foreach(var domainEvent in
                    entry.DomainEventAccessor.GetDomainEvents(entry.Entity))
            {
                var eventEntry = new Dictionary<string, object>
                {
                    { "id", entry.DomainEventAccessor.NextId() },
                    { entry.PartitionKeyName, entry.PartitionKey },
                    { "domainEvent", domainEvent }
                };

                domainEvents.Add(eventEntry);
            }
        }

        foreach (var eventEntry in domainEvents)
            batch.CreateItem(eventEntry, batchOptions);

        var batchResponse = await batch
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);

        for(var i = 0; i < entityEntries.Count; i ++)
        {
            var entry = entityEntries[i];
            var itemResponse = batchResponse[i];

            entry.ReadShadowProperties();
            entry.UpdateETag(itemResponse.ETag);
        }

        return batchResponse;
    }
}
