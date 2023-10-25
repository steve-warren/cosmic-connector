using Cosmodust.Query;
using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;

namespace Cosmodust.Samples.TodoApp.Infra;

public class CosmodustTodoItemRepository : ITodoItemRepository
{
    private readonly DocumentSession _session;

    public CosmodustTodoItemRepository(DocumentSession session)
    {
        _session = session;
    }
    
    public void Add(TodoItem item) =>
        _session.Store(item);

    public void Remove(TodoItem item) =>
        _session.Remove(item);

    public Task<List<TodoItem>> FindByListIdAsync(string ownerId, string listId)
    {
        var query = _session.Query<TodoItem>(
            partitionKey: ownerId,
            sql: "select * from c where c.listId = @listId AND c.__type = 'TodoItem'",
            parameters: new { id = listId });

        return query.ToListAsync();
    }
}
