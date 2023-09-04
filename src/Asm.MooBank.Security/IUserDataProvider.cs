using Asm.MooBank.Models;

namespace Asm.MooBank.Security;

public interface IUserDataProvider : IUserIdProvider
{
    ValueTask<AccountHolder> GetCurrentUser();

}
