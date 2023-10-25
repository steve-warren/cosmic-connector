namespace Cosmodust.Samples.TodoApp.Domain;

public interface IAccountRepository
{
    ValueTask<Account?> FindAsync(string id);
    void Update(Account account);
}
