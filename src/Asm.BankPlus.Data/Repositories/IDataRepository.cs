namespace Asm.BankPlus.Data.Repositories;

public interface IDataRepository
{
    Task<int> SaveChanges();
}
