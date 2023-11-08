namespace Cosmodust.Tests.Domain.Todo;

public partial class TodoItem
{
    public sealed class CompletedState : ICompletionState
    {
        public CompletedState(DateTimeOffset on)
        {
            On = on;
        }

        public string Name { get; } = nameof(CompletedState);
        public DateTimeOffset On { get; }

        public IncompleteState Incomplete()
        {
            return new IncompleteState();
        }
    }
}
