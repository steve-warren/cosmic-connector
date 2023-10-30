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

    public async ValueTask<TodoList?> FindAsync(string ownerId, string id)
    {
        var list = await _session.FindAsync<TodoList>(id: id, partitionKey: ownerId);

        return list;
    }

    public void Add(TodoList list) =>
        _session.Store(list);

    public void Update(TodoList list) =>
        _session.Update(list);
}
