namespace Asm.MooBank.Infrastructure.Repositories;

public abstract class DataRepository : IDataRepository
{
    protected BankPlusContext DataContext { get; }

    protected DataRepository(BankPlusContext dataContext)
    {
        DataContext = dataContext;
    }

    public Task<int> SaveChanges()
    {
        return DataContext.SaveChangesAsync();
    }
}
