using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class AccountHolderRepository(MooBankContext dataContext) : RepositoryBase<AccountHolder, Guid>(dataContext), IAccountHolderRepository
    {
        protected override IQueryable<AccountHolder> GetById(Guid Id)
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder?> GetByCard(short last4Digits, CancellationToken cancellationToken = default) =>
            Entities.SingleOrDefaultAsync(a => a.Cards.Any(c => c.Last4Digits == last4Digits), cancellationToken);
    }
}
