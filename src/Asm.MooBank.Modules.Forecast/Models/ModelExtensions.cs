using System.Text.Json;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Modules.Forecast.Models;

public static class ModelExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static ForecastPlan ToModel(this DomainEntities.ForecastPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        StartDate = plan.StartDate,
        EndDate = plan.EndDate,
        AccountScopeMode = (AccountScopeMode)plan.AccountScopeMode,
        StartingBalanceMode = (StartingBalanceMode)plan.StartingBalanceMode,
        StartingBalanceAmount = plan.StartingBalanceAmount,
        CurrencyCode = plan.CurrencyCode,
        IncomeStrategy = string.IsNullOrEmpty(plan.IncomeStrategySerialized) ? null : JsonSerializer.Deserialize<IncomeStrategy>(plan.IncomeStrategySerialized, JsonOptions),
        OutgoingStrategy = string.IsNullOrEmpty(plan.OutgoingStrategySerialized) ? null : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions),
        Assumptions = string.IsNullOrEmpty(plan.AssumptionsSerialized) ? null : JsonSerializer.Deserialize<Assumptions>(plan.AssumptionsSerialized, JsonOptions),
        IsArchived = plan.IsArchived,
        CreatedUtc = plan.CreatedUtc,
        UpdatedUtc = plan.UpdatedUtc,
        AccountIds = plan.Accounts.Select(a => a.InstrumentId),
        PlannedItems = plan.PlannedItems.Select(i => i.ToModel())
    };

    public static PlannedItem ToModel(this DomainEntities.ForecastPlannedItem item) => new()
    {
        Id = item.Id,
        ItemType = (PlannedItemType)item.ItemType,
        Name = item.Name,
        Amount = item.Amount,
        TagId = item.TagId,
        TagName = item.Tag?.Name,
        VirtualInstrumentId = item.VirtualInstrumentId,
        IsIncluded = item.IsIncluded,
        DateMode = (PlannedItemDateMode)item.DateMode,
        FixedDate = item.FixedDate,
        ScheduleFrequency = item.ScheduleFrequency,
        ScheduleAnchorDate = item.ScheduleAnchorDate,
        ScheduleInterval = item.ScheduleInterval,
        ScheduleDayOfMonth = item.ScheduleDayOfMonth,
        ScheduleEndDate = item.ScheduleEndDate,
        WindowStartDate = item.WindowStartDate,
        WindowEndDate = item.WindowEndDate,
        AllocationMode = item.AllocationMode.HasValue ? (AllocationMode)item.AllocationMode : null,
        Notes = item.Notes
    };

    public static DomainEntities.ForecastPlannedItem ToDomain(this PlannedItem item, Guid planId) => new(item.Id == Guid.Empty ? Guid.NewGuid() : item.Id)
    {
        ForecastPlanId = planId,
        ItemType = (DomainEntities.PlannedItemType)item.ItemType,
        Name = item.Name,
        Amount = item.Amount,
        TagId = item.TagId,
        VirtualInstrumentId = item.VirtualInstrumentId,
        IsIncluded = item.IsIncluded,
        DateMode = (DomainEntities.PlannedItemDateMode)item.DateMode,
        FixedDate = item.FixedDate,
        ScheduleFrequency = item.ScheduleFrequency,
        ScheduleAnchorDate = item.ScheduleAnchorDate,
        ScheduleInterval = item.ScheduleInterval,
        ScheduleDayOfMonth = item.ScheduleDayOfMonth,
        ScheduleEndDate = item.ScheduleEndDate,
        WindowStartDate = item.WindowStartDate,
        WindowEndDate = item.WindowEndDate,
        AllocationMode = item.AllocationMode.HasValue ? (DomainEntities.AllocationMode)item.AllocationMode : null,
        Notes = item.Notes
    };
}
