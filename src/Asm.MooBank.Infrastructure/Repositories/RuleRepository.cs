using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.Repositories;

public class RuleRepository : RepositoryDeleteBase<Rule, int>, IRuleRepository
{

    public RuleRepository(MooBankContext bankPlusContext) : base(bankPlusContext)
    {
    }

    public async Task Delete(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var entity = await DataSet.Include(r => r.Tags).Where(r => r.Id == id && r.AccountId == accountId).SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
        DataContext.Remove(entity);
    }

    public async Task<Rule> Get(Guid accountId, int id, CancellationToken cancellationToken = default)
    {
        var rule = await DataSet.Where(t => t.AccountId == accountId && t.Id == id).SingleOrDefaultAsync(cancellationToken);

        return rule ?? throw new NotFoundException("Rule not found");
    }

    public async Task<IEnumerable<Rule>> GetForAccount(Guid accountId, CancellationToken cancellationToken = default) =>
        await DataSet.Include(t => t.Tags).Where(t => t.AccountId == accountId).OrderBy(t => t.Contains).ToListAsync(cancellationToken);

    protected override IQueryable<Rule> GetById(int id) => DataSet.Where(t => t.Id == id);

}
