using Asm.MooBank.Domain.Entities;
using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IInstitutionAccountRepository : IDeletableRepository<InstitutionAccount, Guid>
{
    Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);
    Task<decimal> GetPosition();
    void AddImportAccount(ImportAccount importAccountEntity);

    void RemoveImportAccount(ImportAccount entity);
}
