using System.Threading;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstitutionAccountRepository(MooBankContext dataContext, User accountHolder) : RepositoryDeleteBase<InstitutionAccount, Guid>(dataContext), IInstitutionAccountRepository
{
    protected override IQueryable<InstitutionAccount> GetById(Guid id) => Entities.Include(a => a.Owners).Include(t => t.ImportAccount).ThenInclude(i => i!.ImporterType).Where(a => a.Id == id && a.Owners.Any(ah => ah.UserId == accountHolder.Id || (a.ShareWithFamily && ah.User.FamilyId == accountHolder.FamilyId)));

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => await Context.Set<ImporterType>().ToListAsync(cancellationToken);

    public async Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<ImporterType>().Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return entity ?? throw new NotFoundException($"Unknown importer type ID {importerTypeId}");
    }

    public void RemoveImportAccount(ImportAccount importAccount)
    {
        Context.Remove(importAccount);
    }

    public Task Load(InstitutionAccount account, CancellationToken cancellationToken) =>
        Context.Entry(account).Reference(a => a.ImportAccount).Query().Include(i => i.ImporterType).LoadAsync(cancellationToken);

    public Task Reload(InstitutionAccount account, CancellationToken cancellationToken) =>
        Context.Entry(account).ReloadAsync(cancellationToken);
}
