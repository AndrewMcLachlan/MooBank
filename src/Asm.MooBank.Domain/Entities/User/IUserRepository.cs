namespace Asm.MooBank.Domain.Entities.User;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByCard(short last4Digits, CancellationToken cancellationToken = default);
}
