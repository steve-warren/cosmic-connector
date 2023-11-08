namespace Cosmodust.Tests.Domain.Todo;

public partial class TodoItem
{
    public interface ICompletionState
    {
        string Name { get; }
    }    
}
