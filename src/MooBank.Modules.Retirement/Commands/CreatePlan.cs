using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Retirement;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Retirement.Models;
using Asm.MooBank.Modules.Retirement.Services;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Retirement.Commands;

[DisplayName("CreateRetirementPlan")]
public record CreatePlan([FromBody] RetirementPlanBase Plan) : ICommand<Models.RetirementPlan>;

internal class CreatePlanHandler(
    IRetirementRepository retirementRepository,
    IUnitOfWork unitOfWork,
    IMemberGuard memberGuard,
    User user) : ICommandHandler<CreatePlan, Models.RetirementPlan>
{
    public async ValueTask<Models.RetirementPlan> Handle(CreatePlan request, CancellationToken cancellationToken)
    {
        await memberGuard.Assert(request.Plan.Members, cancellationToken);

        var entity = Domain.Entities.Retirement.RetirementPlan.Create(user.FamilyId, request.Plan.Name, request.Plan.ToAssumptions());

        foreach (var member in request.Plan.Members)
        {
            entity.AddMember(member.UserId, member.CurrentAge, member.CurrentIncome, member.SalarySacrifice, member.RetirementAge, member.GrowthStrategy, member.AnnualFees, member.InsurancePremium, member.InstrumentIds);
        }

        retirementRepository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
