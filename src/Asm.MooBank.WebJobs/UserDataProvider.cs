using Asm.MooBank.Domain.Entities.AccountHolder;
using Asm.MooBank.Security;

namespace Asm.MooBank.WebJobs
{
    internal class UserDataProvider : IUserDataProvider
    {
        public Guid CurrentUserId => Guid.Empty;

        public Task<AccountHolder> GetCurrentUser()
        {
            throw new NotSupportedException();
        }

        public Task<AccountHolder> GetUser(Guid id)
        {
            throw new NotSupportedException();
        }
    }
}
