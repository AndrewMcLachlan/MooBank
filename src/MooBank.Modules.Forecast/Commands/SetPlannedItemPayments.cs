using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Services;

namespace Asm.MooBank.Modules.Forecast.Commands;

/// <summary>
/// Records which payments belong to a planned item, replacing whatever was linked before.
/// </summary>
[DisplayName("SetPlannedItemPayments")]
public record SetPlannedItemPayments(Guid PlanId, Guid ItemId, PlannedItemPayments Payments) : ICommand<PlannedItem>;

internal class SetPlannedItemPaymentsHandler(IForecastRepository forecastRepository, IPlannedItemMatcher matcher, IUnitOfWork unitOfWork, User user) : ICommandHandler<SetPlannedItemPayments, PlannedItem>
{
    public async ValueTask<PlannedItem> Handle(SetPlannedItemPayments request, CancellationToken cancellationToken)
    {
        var plan = await forecastRepository.Get(request.PlanId, new ForecastPlanDetailsSpecification(), cancellationToken);

        // Owning the plan is not the same as owning the payments. The candidate list only ever
        // offers spending on the plan's own accounts, but nothing made the command agree with it,
        // so any identifier at all could be linked and then read back as a figure in the forecast.
        var outOfScope = await matcher.FindOutOfScope(
            request.Payments.TransactionIds,
            PlanScope.AccountIds(plan, user),
            cancellationToken);

        if (outOfScope.Count > 0)
        {
            throw new NotFoundException("One or more of those payments could not be found");
        }

        plan.SetPlannedItemTransactions(request.ItemId, request.Payments.TransactionIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.PlannedItems.Single(i => i.Id == request.ItemId).ToModel();
    }
}
