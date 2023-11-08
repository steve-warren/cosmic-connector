namespace Cosmodust.Samples.TodoApp.Domain;

public partial class TodoList
{
    public sealed class IncompleteState : ICompletionState
    {
        public string Name { get; } = nameof(IncompleteState);

        public CompletedState Complete(DateTimeOffset completedOn)
        {
            return new CompletedState(completedOn);
        }
    }
}
