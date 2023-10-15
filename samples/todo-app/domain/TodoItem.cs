namespace Cosmodust.Samples.TodoApp.Domain;

public class TodoItem
{
    public TodoItem(string name, string listId, string ownerId, TodoItemPriority priority, string id, string notes, DateTimeOffset? reminder)
    {
        Name = name;
        ListId = listId;
        OwnerId = ownerId;
        Priority = priority;
        Id = id;
        Notes = notes;
        Reminder = reminder;
        State = TodoItemCompletedState.Incomplete;
        ArchiveState = ArchiveState.NotArchived;
    }

    public string Id { get; private set; }
    public string ListId { get; private set; }
    public string OwnerId { get; private set; }
    public string Name { get; private set; } = "";
    public string Notes { get; private set; } = "";
    public DateTimeOffset? Reminder { get; private set; }
    public TodoItemPriority Priority { get; private set; } = TodoItemPriority.None;
    public TodoItemCompletedState State { get; private set; }
    public ArchiveState ArchiveState { get; private set; }

    public void Rename(string newName)
    {
        Name = newName;
    }

    public void Relocate(string listId)
    {
        ListId = listId;
    }

    public void SetReminder(DateTimeOffset? when)
    {
        Reminder = when;
    }

    public void ChangePriority(TodoItemPriority priority)
    {
        Priority = priority;
    }

    public void WriteNotes(string text)
    {
        Notes = text;
    }

    public void ToggleCompletedState()
    {
        if (State == TodoItemCompletedState.Incomplete)
            State = TodoItemCompletedState.Completed;

        else if (State == TodoItemCompletedState.Completed)
            State = TodoItemCompletedState.Incomplete;
    }

    public void Archive()
    {
        ArchiveState = ArchiveState.Archived;
    }
}
