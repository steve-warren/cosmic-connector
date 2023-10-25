namespace Cosmodust.Samples.TodoApp.Domain;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
    Task SaveChangesAsTransactionAsync();
}
