using Asm.MooBank.Models;

namespace Asm.MooBank.Security;

public interface IUserDataProvider : IUserIdProvider
{
    User GetCurrentUser();
    Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default);

}
