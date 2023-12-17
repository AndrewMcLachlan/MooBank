using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.ReferenceData;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstitutionAccountRepository : RepositoryDeleteBase<InstitutionAccount, Guid>, IInstitutionAccountRepository
{
    private readonly IUserDataProvider _userDataProvider;

    public InstitutionAccountRepository(MooBankContext dataContext, IUserDataProvider userDataProvider) : base(dataContext)
    {
        _userDataProvider = userDataProvider;
    }

    public override async Task<IEnumerable<InstitutionAccount>> GetAll(CancellationToken cancellationToken = default)
    {
        return await DataSet.Include(a => a.VirtualAccounts).Where(a => a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)).ToListAsync(cancellationToken);
    }

    protected override IQueryable<InstitutionAccount> GetById(Guid id) => DataSet.Include(a => a.AccountAccountHolders).Include(t => t.ImportAccount).ThenInclude(i => i!.ImporterType).Where(a => a.Id == id && a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId));

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => await DataContext.Set<ImporterType>().ToListAsync(cancellationToken);

    public async Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await DataContext.Set<ImporterType>().Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return entity ?? throw new NotFoundException($"Unknown importer type ID {importerTypeId}");
    }

    public void AddImportAccount(ImportAccount account)
    {
        //DataContext.Add(new ImportAccount);
        throw new NotSupportedException();
    }

    public void RemoveImportAccount(ImportAccount importAccount)
    {
        DataContext.Remove(importAccount);
    }

    public Task<decimal> GetPosition() =>
        DataSet.Where(a => a.AccountAccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId) && a.IncludeInPosition).SumAsync(a => a.Balance);

    public Task Load(InstitutionAccount account, CancellationToken cancellationToken) =>
        DataContext.Entry(account).Reference(a => a.ImportAccount).Query().Include(i => i.ImporterType).LoadAsync(cancellationToken);
}