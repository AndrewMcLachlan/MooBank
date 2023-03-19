using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Security;

public interface IUserDataProvider
{
    Guid CurrentUserId { get; }

    Task<AccountHolder> GetCurrentUser();

}
