namespace Cosmodust.Samples.TodoApp.Domain;

public interface ITodoListRepository
{
    ValueTask<TodoList?> FindAsync(string ownerId, string id);
    void Add(TodoList list);
    void Update(TodoList list);
}
