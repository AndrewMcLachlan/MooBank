using Asm.MooBank.Models;

namespace Asm.MooBank.Security;

public interface IUserDataProvider : IUserIdProvider
{
    AccountHolder GetCurrentUser();
    Task<AccountHolder> GetCurrentUserAsync(CancellationToken cancellationToken = default);

}
