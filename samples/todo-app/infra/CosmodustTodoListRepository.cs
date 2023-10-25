using Cosmodust.Samples.TodoApp.Domain;
using Cosmodust.Session;

namespace Cosmodust.Samples.TodoApp.Infra;

public class CosmodustTodoListRepository : ITodoListRepository
{
    private readonly DocumentSession _session;

    public CosmodustTodoListRepository(DocumentSession session)
    {
        _session = session;
    }

    public ValueTask<TodoList?> FindAsync(string ownerId, string id) =>
        _session.FindAsync<TodoList>(id: id, partitionKey: ownerId);

    public void Add(TodoList list) =>
        _session.Store(list);

    public void Update(TodoList list) =>
        _session.Update(list);
}
