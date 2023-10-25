namespace Cosmodust.Samples.TodoApp.Domain;

public interface ITodoItemRepository
{
    void Add(TodoItem item);
    void Remove(TodoItem item);
    Task<List<TodoItem>> FindByListIdAsync(string ownerId, string listId);
}
