using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Security;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class AccountHolderRepository(MooBankContext dataContext, IUserDataProvider userDataProvider) : RepositoryBase<AccountHolder, Guid>(dataContext), IAccountHolderRepository
    {
        private readonly IUserDataProvider _userDataProvider = userDataProvider;

        [Obsolete("Not used")]
        public Task<AccountHolder?> GetCurrentOrNull(CancellationToken cancellationToken = default) =>
            DataContext.AccountHolders.SingleOrDefaultAsync(a => a.AccountHolderId == _userDataProvider.CurrentUserId, cancellationToken);

        [Obsolete("Not used")]
        public Task<AccountHolder> GetCurrent(CancellationToken cancellationToken = default) =>
            (DataContext.AccountHolders.SingleOrDefaultAsync(a => a.AccountHolderId == _userDataProvider.CurrentUserId, cancellationToken) ?? throw new NotAuthorisedException())!;


        protected override IQueryable<AccountHolder> GetById(Guid Id)
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder?> GetByCard(short last4Digits, CancellationToken cancellationToken = default) =>
            DataSet.SingleOrDefaultAsync(a => a.Cards.Any(c => c.Last4Digits == last4Digits), cancellationToken);
    }
}
