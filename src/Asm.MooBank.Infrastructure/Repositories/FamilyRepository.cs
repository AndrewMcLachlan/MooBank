using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Family;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class FamilyRepository(MooBankContext context) : RepositoryWriteBase<MooBankContext, Family, Guid>(context), IFamilyRepository
{
}
