namespace Cosmodust.Samples.TodoApp.Domain;

public record ArchiveState
{
    public static readonly ArchiveState NotArchived = new() { Name = nameof(NotArchived) };
    public static readonly ArchiveState Archived = new() { Name = nameof(Archived) };

    public static ArchiveState Parse(string name)
    {
        return name switch
        {
            nameof(NotArchived) => NotArchived,
            nameof(Archived) => Archived,
            _ => throw new ArgumentException("invalid state", nameof(name))
        };
    }

    private ArchiveState() { }

    public string Name { get; init; } = "";
}
