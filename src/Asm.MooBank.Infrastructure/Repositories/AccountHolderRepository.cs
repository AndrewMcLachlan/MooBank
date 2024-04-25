using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class AccountHolderRepository(MooBankContext dataContext) : RepositoryBase<User, Guid>(dataContext), IAccountHolderRepository
    {
        protected override IQueryable<User> GetById(Guid Id)
        {
            throw new NotSupportedException();
        }

        public Task<User?> GetByCard(short last4Digits, CancellationToken cancellationToken = default) =>
            Entities.SingleOrDefaultAsync(a => a.Cards.Any(c => c.Last4Digits == last4Digits), cancellationToken);
    }
}
