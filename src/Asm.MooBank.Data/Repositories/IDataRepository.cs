namespace Asm.MooBank.Data.Repositories;

public interface IDataRepository
{
    Task<int> SaveChanges();
}
