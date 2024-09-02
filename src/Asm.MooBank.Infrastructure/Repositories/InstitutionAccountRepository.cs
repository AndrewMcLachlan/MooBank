using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Events;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Models;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstitutionAccountRepository(MooBankContext dataContext, User user) : RepositoryDeleteBase<InstitutionAccount, Guid>(dataContext), IInstitutionAccountRepository
{
    public InstitutionAccount Add(InstitutionAccount entity, decimal openingBalance)
    {
        var tracked = base.Add(entity);
        tracked.Events.Add(new AccountAddedEvent(tracked, openingBalance));
        return tracked;
    }

    protected override IQueryable<InstitutionAccount> GetById(Guid id) => Entities.Include(a => a.Owners).Include(t => t.ImportAccount).ThenInclude(i => i!.ImporterType).Where(a => a.Id == id && a.Owners.Any(ah => ah.UserId == user.Id || (a.ShareWithFamily && ah.User.FamilyId == user.FamilyId)));

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => await Context.ImporterTypes.ToListAsync(cancellationToken);

    public async Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await Context.ImporterTypes.Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

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
