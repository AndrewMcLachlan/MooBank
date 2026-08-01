using Asm.Domain.Infrastructure;
using Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Infrastructure.Repositories;

internal class RetirementRepository(MooBankContext context) : RepositoryDeleteBase<MooBankContext, RetirementPlan, Guid>(context), IRetirementRepository
{
    public override void Delete(Guid id)
    {
        var plan = Entities.Find(id) ?? throw new NotFoundException();
        Entities.Remove(plan);
    }
}
