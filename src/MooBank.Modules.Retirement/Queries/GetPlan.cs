using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Queries;

public record GetPlan(Guid Id) : IQuery<Models.RetirementPlan>;

internal class GetPlanHandler(IQueryable<DomainEntities.RetirementPlan> plans, User user) : IQueryHandler<GetPlan, Models.RetirementPlan>
{
    public async ValueTask<Models.RetirementPlan> Handle(GetPlan query, CancellationToken cancellationToken)
    {
        // Specified, so the members arrive with the people they name. Without it the plan comes back
        // with members whose user was never loaded, and every one of them reads as unnamed.
        var plan = await plans
            .Specify(new RetirementPlanDetailsSpecification())
            .SingleOrDefaultAsync(p => p.Id == query.Id && p.FamilyId == user.FamilyId, cancellationToken) ??
            throw new NotFoundException("Retirement plan not found");

        return plan.ToModel();
    }
}
