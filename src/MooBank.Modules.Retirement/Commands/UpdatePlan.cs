using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Domain.Entities.Retirement.Specifications;
using Asm.MooBank.Modules.Retirement.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Retirement.Commands;

[DisplayName("UpdateRetirementPlan")]
public record UpdatePlan(Guid Id, [FromBody] RetirementPlanBase Plan) : ICommand<Models.RetirementPlan>;

internal class UpdatePlanHandler(
    IRetirementRepository retirementRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePlan, Models.RetirementPlan>
{
    public async ValueTask<Models.RetirementPlan> Handle(UpdatePlan request, CancellationToken cancellationToken)
    {
        var entity = await retirementRepository.Get(request.Id, new RetirementPlanDetailsSpecification(), cancellationToken);

        entity.Update(request.Plan.Name, request.Plan.ToAssumptions());

        ReconcileMembers(entity, request.Plan.Members);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    /// <summary>
    /// Bring the plan's members into line with the supplied set: members carrying an id are
    /// updated, members without one are added, and any member the caller left out is removed.
    /// </summary>
    /// <exception cref="NotFoundException">
    /// Thrown when a supplied id does not belong to this plan. Silently treating it as a new member
    /// would hide a caller sending an id from a different plan.
    /// </exception>
    private static void ReconcileMembers(Domain.Entities.Retirement.RetirementPlan entity, IEnumerable<Models.RetirementPlanMember> members)
    {
        var supplied = members.ToList();
        var suppliedIds = supplied.Where(m => m.Id.HasValue).Select(m => m.Id!.Value).ToHashSet();

        foreach (var removed in entity.Members.Where(m => !suppliedIds.Contains(m.Id)).ToList())
        {
            entity.RemoveMember(removed.Id);
        }

        foreach (var member in supplied)
        {
            if (member.Id is null)
            {
                entity.AddMember(member.Name, member.CurrentAge, member.CurrentIncome, member.SalarySacrifice, member.RetirementAge, member.GrowthStrategy, member.InstrumentIds);
                continue;
            }

            var existing = entity.Members.SingleOrDefault(m => m.Id == member.Id.Value) ??
                throw new NotFoundException($"Member {member.Id} does not belong to this plan");

            existing.Update(member.Name, member.CurrentAge, member.CurrentIncome, member.SalarySacrifice, member.RetirementAge, member.GrowthStrategy);
            existing.SetAccounts(member.InstrumentIds);
        }
    }
}
