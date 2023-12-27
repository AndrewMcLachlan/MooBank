using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class AccountHolderRepository(MooBankContext dataContext, IUserDataProvider userDataProvider) : RepositoryBase<AccountHolder, Guid>(dataContext), IAccountHolderRepository
    {
        private readonly IUserDataProvider _userDataProvider = userDataProvider;

        protected override IQueryable<AccountHolder> GetById(Guid Id)
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder?> GetByCard(short last4Digits, CancellationToken cancellationToken = default) =>
            Entities.SingleOrDefaultAsync(a => a.Cards.Any(c => c.Last4Digits == last4Digits), cancellationToken);
    }
}
