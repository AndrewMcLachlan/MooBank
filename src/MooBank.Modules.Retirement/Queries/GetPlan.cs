using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Queries;

public record GetPlan(Guid Id) : IQuery<Models.RetirementPlan>;

internal class GetPlanHandler(IQueryable<DomainEntities.RetirementPlan> plans, User user) : IQueryHandler<GetPlan, Models.RetirementPlan>
{
    public async ValueTask<Models.RetirementPlan> Handle(GetPlan query, CancellationToken cancellationToken)
    {
        var plan = await plans
            .SingleOrDefaultAsync(p => p.Id == query.Id && p.FamilyId == user.FamilyId, cancellationToken) ??
            throw new NotFoundException("Retirement plan not found");

        return plan.ToModel();
    }
}
