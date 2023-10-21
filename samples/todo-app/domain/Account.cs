namespace Cosmodust.Samples.TodoApp.Domain;

public class Account
{
    public Account(string id)
    {
        Id = id;
        OwnerId = id;
    }

    public string Id { get; init; }
    public string OwnerId { get; init; }
    public int NumberOfLists { get; private set; }

    public void AddList() =>
        NumberOfLists++;

    public void RemoveList() =>
        NumberOfLists--;
}
