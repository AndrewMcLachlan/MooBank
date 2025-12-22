using Asm.MooBank.Models;

namespace Asm.MooBank.Security;

public interface IUserDataProvider : IUserIdProvider
{
    User GetCurrentUser();
}

/// <summary>
/// A user data provider that can be set programmatically for background processing.
/// </summary>
public interface ISettableUserDataProvider : IUserDataProvider
{
    void SetUser(User user);
}
