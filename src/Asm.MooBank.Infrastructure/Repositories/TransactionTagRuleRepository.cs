using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

public class TransactionTagRuleRepository : RepositoryDeleteBase<TransactionTagRule, int>, ITransactionTagRuleRepository
{

    public TransactionTagRuleRepository(MooBankContext bankPlusContext) : base(bankPlusContext)
    {
    }

    public async Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var entity = await DataSet.Include(r => r.TransactionTags).Where(r => r.TransactionTagRuleId == id && r.AccountId == accountId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
        DataContext.Remove(entity);
    }

    public async Task<TransactionTagRule> Get(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var rule = await DataSet.Where(t => t.AccountId == accountId && t.TransactionTagRuleId == id).SingleOrDefaultAsync(cancellationToken);

        return rule ?? throw new NotFoundException("Rule not found");
    }

    public async Task<IEnumerable<TransactionTagRule>> GetForAccount(Guid accountId, CancellationToken cancellationToken = default) =>
        await DataSet.Include(t => t.TransactionTags).Where(t => t.AccountId == accountId).OrderBy(t => t.Contains).ToListAsync(cancellationToken);

    protected override IQueryable<TransactionTagRule> GetById(int id) => DataSet.Where(t => t.TransactionTagRuleId == id);

}
