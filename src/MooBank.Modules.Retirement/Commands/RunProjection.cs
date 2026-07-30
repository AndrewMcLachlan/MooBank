using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Microsoft.AspNetCore.Mvc;
using DomainEntities = Asm.MooBank.Domain.Entities.Retirement;

namespace Asm.MooBank.Modules.Retirement.Commands;

[DisplayName("RunRetirementProjection")]
public record RunProjection(Guid PlanId, [FromBody] ProjectionOverrides? Overrides = null) : ICommand<RetirementProjection>;

internal class RunProjectionHandler(
    IQueryable<DomainEntities.RetirementPlan> plans,
    IRetirementProjectionEngine projectionEngine,
    IPensionRateReader pensionRateReader,
    MooBank.Models.User user) : ICommandHandler<RunProjection, RetirementProjection>
{
    public async ValueTask<RetirementProjection> Handle(RunProjection command, CancellationToken cancellationToken)
    {
        var plan = await plans
            .Specify(new RetirementPlanProjectionSpecification())
            .SingleOrDefaultAsync(p => p.Id == command.PlanId && p.FamilyId == user.FamilyId, cancellationToken) ??
            throw new NotFoundException("Retirement plan not found");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var pensionRates = await pensionRateReader.Current(today, cancellationToken);

        return projectionEngine.Calculate(plan, today, pensionRates, command.Overrides);
    }
}
