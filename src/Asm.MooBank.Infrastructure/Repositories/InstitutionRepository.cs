using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Institution;

namespace Asm.MooBank.Infrastructure.Repositories;
internal class InstitutionRepository(MooBankContext context) : RepositoryBase<MooBankContext, Institution, int>(context), IInstitutionRepository
{
}
