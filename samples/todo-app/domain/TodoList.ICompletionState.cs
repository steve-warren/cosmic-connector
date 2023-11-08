namespace Cosmodust.Samples.TodoApp.Domain;

public partial class TodoList
{
    public interface ICompletionState
    {
        string Name { get; }
    }
}
