namespace Cosmodust.Samples.TodoApp.Domain;

public partial class TodoList
{
    public TodoList(string id,
                    string name,
                    string ownerId)
    {
        Id = id;
        Name = name;
        OwnerId = ownerId;
        ArchiveState = ArchiveState.NotArchived;
        CompletionState = new IncompleteState();
    }

    public string Id { get; private set; }
    public string OwnerId { get; private set; }
    public string Name { get; private set; }
    public int Count => Items.Count;
    public List<string> Items { get; private set; } = new();
    public ArchiveState ArchiveState { get; private set; }
    public ICompletionState CompletionState { get; private set; }
    
    public void SetState(ICompletionState completionState)
    {
        CompletionState = completionState;
    }
    
    public void Rename(string newName)
    {
        Name = newName;
    }

    public void AddItem(TodoItem item)
    {
        if (Items.Contains(item.Id))
            throw new InvalidOperationException(
                "Attempting to add item to list when it already exists.");

        Items.Insert(index: 0, item.Id);
    }

    public void RemoveItem(TodoItem item)
        => Items.Remove(item.Id);

    public void Archive()
    {
        ArchiveState = ArchiveState.Archived;
    }
}
