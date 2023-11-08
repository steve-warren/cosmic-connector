namespace Cosmodust.Tests.Domain.Todo;

public partial class TodoItem
{
    public sealed class IncompleteState : ICompletionState
    {
        public string Name { get; } = nameof(IncompleteState);

        public CompletedState Complete(DateTimeOffset on)
        {
            return new CompletedState(on);
        }
    }
}
