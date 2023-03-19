using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories;

public class InstitutionAccountRepository : RepositoryDeleteBase<InstitutionAccount, Guid>, IInstitutionAccountRepository
{
    private readonly IUserDataProvider _userDataProvider;

    public InstitutionAccountRepository(BankPlusContext dataContext, IUserDataProvider userDataProvider) : base(dataContext)
    {
        _userDataProvider = userDataProvider;
    }

    public override async Task<IEnumerable<InstitutionAccount>> GetAll(CancellationToken cancellationToken = default)
    {
        return await DataSet.Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId)).ToListAsync(cancellationToken);
    }

    protected override IQueryable<InstitutionAccount> GetById(Guid id) => DataSet.Where(a => a.AccountId == id && a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId));

    public async Task<IEnumerable<ImporterType>> GetImporterTypes(CancellationToken cancellationToken = default) => await DataContext.Set<ImporterType>().ToListAsync(cancellationToken);

    public async Task<ImporterType> GetImporterType(int importerTypeId, CancellationToken cancellationToken = default)
    {
        var entity = await DataContext.Set<ImporterType>().Where(i => i.ImporterTypeId == importerTypeId).SingleOrDefaultAsync();

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
        DataSet.Where(a => a.AccountHolders.Any(ah => ah.AccountHolderId == _userDataProvider.CurrentUserId) && a.IncludeInPosition).SumAsync(a => a.Balance);


}