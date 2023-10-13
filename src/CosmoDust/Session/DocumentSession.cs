using System.Diagnostics;
using System.Runtime.CompilerServices;
using CosmoDust.Linq;

namespace CosmoDust;

public sealed class DocumentSession : IDocumentSession
{
    internal DocumentSession(DocumentStore documentStore, EntityConfigurationHolder entityConfiguration, IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(documentStore);
        ArgumentNullException.ThrowIfNull(entityConfiguration);

        DocumentStore = documentStore;
        Database = database;
        EntityConfiguration = entityConfiguration;

        ChangeTracker = new ChangeTracker(entityConfiguration);
    }

    public ChangeTracker ChangeTracker { get; }
    public DocumentStore DocumentStore { get; }
    public IDatabase Database { get; }
    public EntityConfigurationHolder EntityConfiguration { get; }

    public async ValueTask<TEntity?> FindAsync<TEntity>(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var configuration = DocumentStore.GetConfiguration(typeof(TEntity));
        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

        if (ChangeTracker.TryGet(id, out TEntity? entity))
            return entity;

        entity = await Database.FindAsync<TEntity>(configuration.ContainerName, id, partitionKey, cancellationToken);

        if (entity is null)
            return default;

        ChangeTracker.RegisterUnchanged(entity);

        return entity;
    }

    public IQueryable<TEntity> Query<TEntity>(string? partitionKey = null)
    {
        var config = DocumentStore.GetConfiguration(typeof(TEntity));
        return new DocumentQuery<TEntity>(this, Database.GetLinqQuery<TEntity>(config.ContainerName, partitionKey));
    }

    public async IAsyncEnumerable<TEntity> GetAsyncEnumerable<TEntity>(IQueryable<TEntity> queryable, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(queryable);

        var query = Database.GetAsyncEnumerable(queryable, cancellationToken);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            Debug.Assert(entity is not null, "The entity should not be null.");

            ChangeTracker.RegisterUnchanged(entity);

            yield return entity;
        }
    }

    public void Store<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.RegisterAdded(entity);
    }

    public void Update<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.EnsureExists(entity);
        ChangeTracker.RegisterModified(entity);
    }

    public void Remove<TEntity>(TEntity entity)
    {
        DocumentStore.EnsureConfigured<TEntity>();
        ArgumentNullException.ThrowIfNull(entity);

        ChangeTracker.EnsureExists(entity);
        ChangeTracker.RegisterRemoved(entity);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        ChangeTracker.Commit();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(ChangeTracker.PendingChanges, cancellationToken).ConfigureAwait(false);

        ChangeTracker.Commit();
    }
}
