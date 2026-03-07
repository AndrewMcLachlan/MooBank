namespace Asm.MooBank.Security;

public interface IUserIdProvider
{
    Guid CurrentUserId { get; }
}
