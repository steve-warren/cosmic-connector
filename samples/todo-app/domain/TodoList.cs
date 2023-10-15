namespace Cosmodust.Samples.TodoApp.Domain;

public class TodoList
{
    public TodoList(string name, string id, string ownerId)
    {
        Name = name;
        Id = id;
        OwnerId = ownerId;
    }

    public string Id { get; private set; }
    public string OwnerId { get; private set; }
    public string Name { get; private set; } = "";
}
