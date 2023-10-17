namespace Cosmodust.Samples.TodoApp.Domain;

public record TodoItemCompletedState
{
    public static TodoItemCompletedState Parse(string name)
    {
        return name switch
        {
            nameof(Incomplete) => Incomplete,
            nameof(Completed) => Completed,
            _ => throw new ArgumentException("invalid state", nameof(name))
        };
    }

    public static readonly TodoItemCompletedState Incomplete = new() { Name = nameof(Incomplete) };
    public static readonly TodoItemCompletedState Completed = new() { Name = nameof(Completed) };

    private TodoItemCompletedState() { }

    public string Name { get; init; } = "";
}
