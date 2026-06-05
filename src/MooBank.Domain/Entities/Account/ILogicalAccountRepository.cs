namespace Asm.MooBank.Domain.Entities.Account;

public interface ILogicalAccountRepository : IDeletableRepository<LogicalAccount, Guid>, IWritableRepository<LogicalAccount, Guid>
{
    LogicalAccount Add(LogicalAccount entity, decimal openingBalance, DateOnly openedDate);

    void RemoveImportAccount(ImportAccount entity);
}
