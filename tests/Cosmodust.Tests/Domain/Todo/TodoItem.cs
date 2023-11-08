namespace Cosmodust.Tests.Domain.Todo;

public partial class TodoItem
{
    public TodoItem(string id)
    {
        Id = id;
        CompletionState = new IncompleteState();
    }

    public string Id { get; }
    public ICompletionState CompletionState { get; private set; }

    public void ChangeCompletionState(ICompletionState completionState)
    {
        CompletionState = completionState;
    }
}
