namespace Cosmodust.Samples.TodoApp.Domain;

public record TodoItemPriority
{
    public static TodoItemPriority Parse(string name)
    {
        return name switch
        {
            nameof(None) => None,
            nameof(Low) => Low,
            nameof(Medium) => Medium,
            nameof(High) => High,
            _ => throw new ArgumentException("invalid priority", nameof(name))
        };
    }

    public static readonly TodoItemPriority None = new() { Name = nameof(None) };
    public static readonly TodoItemPriority Low = new() { Name = nameof(Low) };
    public static readonly TodoItemPriority Medium = new() { Name = nameof(Medium) };
    public static readonly TodoItemPriority High = new() { Name = nameof(High) };

    private TodoItemPriority() { }

    public string Name { get; init; } = "";
    
    public override string ToString() =>
        Name;
}
