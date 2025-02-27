using Asm.MooBank.Domain.Entities.Utility;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class UtilityAccountRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, Account, Guid>(context), IAccountRepository
{
}
