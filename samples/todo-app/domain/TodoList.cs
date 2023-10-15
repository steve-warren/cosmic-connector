namespace Cosmodust.Samples.TodoApp.Domain;

public class TodoList
{
    public TodoList(string id,
                    string name,
                    string ownerId)
    {
        Id = ListId = id;
        Name = name;
        OwnerId = ownerId;
        ArchiveState = ArchiveState.NotArchived;
    }

    public string Id { get; private set; }
    public string OwnerId { get; private set; }
    public string ListId { get; private set; }
    public string Name { get; private set; } = "";
    private List<string> Items { get; init; } = new();
    public ArchiveState ArchiveState { get; private set; }

    public void Rename(string newName)
    {
        Name = newName;
    }

    public void AddItem(string itemId)
    {
        if (Items.Contains(itemId))
            throw new InvalidOperationException("Attempting to add item to list when it already exists.");

        Items.Insert(index: 0, itemId);
    }

    public void ArrangeItem(string itemId, int position)
    {
        var currentIndex = Items.IndexOf(itemId);

        if (currentIndex is -1) return;
        if (position < 0 || position > Items.Count - 1) throw new ArgumentOutOfRangeException(nameof(position));

        Items.RemoveAt(currentIndex);
        Items.Insert(index: position, itemId);
    }

    public void Archive()
    {
        ArchiveState = ArchiveState.Archived;
    }
}
