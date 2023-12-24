using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstitutionAccountRepository(MooBankContext dataContext, AccountHolder accountHolder) : RepositoryDeleteBase<InstitutionAccount, Guid>(dataContext), IInstitutionAccountRepository
{
    protected override IQueryable<InstitutionAccount> GetById(Guid id) => DataSet.Include(a => a.AccountHolders).Include(t => t.ImportAccount).ThenInclude(i => i!.ImporterType).Where(a => a.Id == id && a.AccountHolders.Any(ah => ah.AccountHolderId == accountHolder.Id || (a.ShareWithFamily && ah.AccountHolder.FamilyId == accountHolder.FamilyId)));

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => await DataContext.Set<ImporterType>().ToListAsync(cancellationToken);

    public async Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await DataContext.Set<ImporterType>().Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return entity ?? throw new NotFoundException($"Unknown importer type ID {importerTypeId}");
    }

    public void RemoveImportAccount(ImportAccount importAccount)
    {
        DataContext.Remove(importAccount);
    }

    public Task Load(InstitutionAccount account, CancellationToken cancellationToken) =>
        DataContext.Entry(account).Reference(a => a.ImportAccount).Query().Include(i => i.ImporterType).LoadAsync(cancellationToken);
}