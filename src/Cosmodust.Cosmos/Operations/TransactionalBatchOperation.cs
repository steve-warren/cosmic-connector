using System.Diagnostics;
using Cosmodust.Tracking;
using Microsoft.Azure.Cosmos;

namespace Cosmodust.Cosmos.Operations;

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

        foreach (var entry in entries)
        {
            // return the shadow property to the store
            // for the json serializer to pick up
            entry.ReturnShadowPropertiesToStore();

            _ = entry.State switch
            {
                EntityState.Added => batch.CreateItem(entry.Entity),
                EntityState.Removed => batch.DeleteItem(entry.Id),
                EntityState.Modified => batch.ReplaceItem(entry.Id, entry.Entity),
                EntityState.Unchanged or
                    EntityState.Detached => batch,
                _ => throw new InvalidOperationException()
            };
        }

        var response = await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        Debug.WriteLine(
            $"Transaction operation HTTP {response.StatusCode} - RUs {response.Headers.RequestCharge}");

        return response;
    }
}
