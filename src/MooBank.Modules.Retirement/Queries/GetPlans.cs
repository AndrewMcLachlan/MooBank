using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Queries;

public record GetPlans() : IQuery<IEnumerable<Models.RetirementPlan>>;

internal class GetPlansHandler(IQueryable<DomainEntities.RetirementPlan> plans, User user) : IQueryHandler<GetPlans, IEnumerable<Models.RetirementPlan>>
{
    public async ValueTask<IEnumerable<Models.RetirementPlan>> Handle(GetPlans query, CancellationToken cancellationToken)
    {
        var result = await plans
            .Specify(new RetirementPlanDetailsSpecification())
            .Where(p => p.FamilyId == user.FamilyId)
            .OrderByDescending(p => p.UpdatedUtc)
            .ToListAsync(cancellationToken);

        return result.ToModel();
    }
}
