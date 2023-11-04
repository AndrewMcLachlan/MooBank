using Asm.MooBank.Models;

namespace Asm.MooBank.Web.Jobs
{
    internal class UserDataProvider : IUserDataProvider
    {
        public Guid CurrentUserId => Guid.Empty;

        public AccountHolder GetCurrentUser()
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder> GetUser(Guid id)
        {
            throw new NotSupportedException();
        }
    }
}
