namespace Cosmodust.Tests.Domain.Todo;

public class TodoItem
{
    private ICompletionState _completionState;

    public TodoItem()
    {
        _completionState = new IncompleteState(this);
    }

    public required string Id { get; init; }

    public ICompletionState CompletionState
    {
        get => _completionState;
        set
        {
            if (value is null)
                throw new ArgumentNullException();

            var setter = value as IBaseTypeSetter;
            setter?.SetBaseType(this);

            _completionState = value;
        }
    }

    public interface ICompletionState
    {

    }

    private interface IBaseTypeSetter
    {
        void SetBaseType(TodoItem item);
    }

    public sealed class IncompleteState : ICompletionState, IBaseTypeSetter
    {
        private TodoItem? _item;

        public IncompleteState() { }

        public IncompleteState(TodoItem item) =>
            _item = item;

        public void SetBaseType(TodoItem item)
        {
            _item = item;
        }
    }

    public sealed class CompletedState : ICompletionState, IBaseTypeSetter
    {
        private TodoItem? _item;

        public CompletedState() { }

        public CompletedState(TodoItem item) =>
            _item = item;

        public void SetBaseType(TodoItem item)
        {
            _item = item;
        }
    }
}
