using System.ComponentModel;
using System.Text.Json;
using Asm.MooBank.Domain.Entities.Forecast;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Modules.Forecast.Models;

namespace Asm.MooBank.Modules.Forecast.Commands;

[DisplayName("UpdateForecastPlan")]
public record UpdatePlan(Guid Id, Models.ForecastPlan Plan) : ICommand<Models.ForecastPlan>;

internal class UpdatePlanHandler(IForecastRepository forecastRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<UpdatePlan, Models.ForecastPlan>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async ValueTask<Models.ForecastPlan> Handle(UpdatePlan request, CancellationToken cancellationToken)
    {
        var entity = await forecastRepository.Get(request.Id, new ForecastPlanDetailsSpecification(), cancellationToken);

        await security.AssertFamilyPermission(entity.FamilyId);

        entity.Name = request.Plan.Name;
        entity.StartDate = request.Plan.StartDate;
        entity.EndDate = request.Plan.EndDate;
        entity.AccountScopeMode = request.Plan.AccountScopeMode;
        entity.StartingBalanceMode = request.Plan.StartingBalanceMode;
        entity.StartingBalanceAmount = request.Plan.StartingBalanceAmount;
        entity.CurrencyCode = request.Plan.CurrencyCode;
        entity.IncomeStrategySerialized = request.Plan.IncomeStrategy != null ? JsonSerializer.Serialize(request.Plan.IncomeStrategy, JsonOptions) : null;
        entity.OutgoingStrategySerialized = request.Plan.OutgoingStrategy != null ? JsonSerializer.Serialize(request.Plan.OutgoingStrategy, JsonOptions) : null;
        entity.AssumptionsSerialized = request.Plan.Assumptions != null ? JsonSerializer.Serialize(request.Plan.Assumptions, JsonOptions) : null;
        entity.UpdatedUtc = DateTime.UtcNow;

        entity.SetAccounts(request.Plan.AccountIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
