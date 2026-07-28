using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Commands;

[DisplayName("RunRetirementProjection")]
public record RunProjection(Guid PlanId) : ICommand<RetirementProjection>;

internal class RunProjectionHandler(
    IQueryable<DomainEntities.RetirementPlan> plans,
    IRetirementProjectionEngine projectionEngine,
    MooBank.Models.User user) : ICommandHandler<RunProjection, RetirementProjection>
{
    public async ValueTask<RetirementProjection> Handle(RunProjection command, CancellationToken cancellationToken)
    {
        var plan = await plans
            .Specify(new RetirementPlanProjectionSpecification())
            .SingleOrDefaultAsync(p => p.Id == command.PlanId && p.FamilyId == user.FamilyId, cancellationToken) ??
            throw new NotFoundException("Retirement plan not found");

        return projectionEngine.Calculate(plan, DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
