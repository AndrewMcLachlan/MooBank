using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Security;

public interface IUserDataProvider : IUserIdProvider
{
    Task<AccountHolder> GetCurrentUser();

}
