﻿using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Infrastructure.Repositories
{
    public class UserRepository(MooBankContext dataContext) : RepositoryBase<MooBankContext, User, Guid>(dataContext), IUserRepository
    {
        public Task<User?> GetByCard(short last4Digits, CancellationToken cancellationToken = default) =>
            Entities.SingleOrDefaultAsync(a => a.Cards.Any(c => c.Last4Digits == last4Digits), cancellationToken);
    }
}
