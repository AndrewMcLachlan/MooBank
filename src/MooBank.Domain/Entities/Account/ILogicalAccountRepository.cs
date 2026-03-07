using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Domain.Entities.Account;

public interface ILogicalAccountRepository : IDeletableRepository<LogicalAccount, Guid>, IWritableRepository<LogicalAccount, Guid>
{
    LogicalAccount Add(LogicalAccount entity, decimal openingBalance, DateOnly openedDate);

    Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default);

    void RemoveImportAccount(ImportAccount entity);
}
