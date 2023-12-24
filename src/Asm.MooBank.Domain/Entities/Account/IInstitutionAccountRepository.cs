using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Domain.Repositories;

namespace Asm.MooBank.Domain.Entities.Account;

public interface IInstitutionAccountRepository : IDeletableRepository<InstitutionAccount, Guid>
{
    Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default);
    void RemoveImportAccount(ImportAccount entity);

    Task Load(InstitutionAccount account, CancellationToken cancellationToken = default);
}
