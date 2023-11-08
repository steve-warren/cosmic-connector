namespace Cosmodust.Samples.TodoApp.Domain;

public partial class TodoList
{
    public sealed class CompletedState : ICompletionState
    {
        public CompletedState(DateTimeOffset completedOn)
        {
            CompletedOn = completedOn;
        }

        public string Name { get; } = nameof(CompletedState);
        public DateTimeOffset CompletedOn { get; }

        public IncompleteState Incomplete()
        {
            return new IncompleteState();
        }
    }
}
