using System.Text.Json;
using Asm.MooBank.Models;
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
        AccountScopeMode = plan.AccountScopeMode,
        StartingBalanceMode = plan.StartingBalanceMode,
        StartingBalanceAmount = plan.StartingBalanceAmount,
        CurrencyCode = plan.CurrencyCode,
        IncomeStrategy = String.IsNullOrEmpty(plan.IncomeStrategySerialized) ? null : JsonSerializer.Deserialize<IncomeStrategy>(plan.IncomeStrategySerialized, JsonOptions),
        OutgoingStrategy = String.IsNullOrEmpty(plan.OutgoingStrategySerialized) ? null : JsonSerializer.Deserialize<OutgoingStrategy>(plan.OutgoingStrategySerialized, JsonOptions),
        Assumptions = String.IsNullOrEmpty(plan.AssumptionsSerialized) ? null : JsonSerializer.Deserialize<Assumptions>(plan.AssumptionsSerialized, JsonOptions),
        IsArchived = plan.IsArchived,
        CreatedUtc = plan.CreatedUtc,
        UpdatedUtc = plan.UpdatedUtc,
        AccountIds = plan.Accounts.Select(a => a.InstrumentId),
        PlannedItems = plan.PlannedItems.Select(i => i.ToModel())
    };

    public static PlannedItem ToModel(this DomainEntities.ForecastPlannedItem item) => new()
    {
        Id = item.Id,
        ItemType = item.ItemType,
        Name = item.Name,
        Amount = item.Amount,
        TagId = item.TagId,
        TagName = item.Tag?.Name,
        VirtualInstrumentId = item.VirtualInstrumentId,
        IsIncluded = item.IsIncluded,
        DateMode = item.DateMode,
        // Map from navigation properties
        FixedDate = item.FixedDate?.FixedDate,
        ScheduleFrequency = item.Schedule?.Frequency,
        ScheduleAnchorDate = item.Schedule?.AnchorDate,
        ScheduleInterval = item.Schedule?.Interval,
        ScheduleDayOfMonth = item.Schedule?.DayOfMonth,
        ScheduleEndDate = item.Schedule?.EndDate,
        WindowStartDate = item.FlexibleWindow?.StartDate,
        WindowEndDate = item.FlexibleWindow?.EndDate,
        AllocationMode = item.FlexibleWindow?.AllocationMode,
        Notes = item.Notes
    };

    public static DomainEntities.ForecastPlannedItem ToDomain(this PlannedItemBase item, Guid planId)
    {
        var id = Guid.NewGuid();

        var entity = new DomainEntities.ForecastPlannedItem(id)
        {
            ForecastPlanId = planId,
            ItemType = item.ItemType,
            Name = item.Name,
            Amount = item.Amount,
            TagId = item.TagId,
            VirtualInstrumentId = item.VirtualInstrumentId,
            IsIncluded = item.IsIncluded,
            DateMode = item.DateMode,
            Notes = item.Notes
        };

        // Set navigation properties based on DateMode
        switch (item.DateMode)
        {
            case PlannedItemDateMode.FixedDate when item.FixedDate.HasValue:
                entity.FixedDate = new DomainEntities.PlannedItemFixedDate
                {
                    PlannedItemId = id,
                    FixedDate = item.FixedDate.Value
                };
                break;

            case PlannedItemDateMode.Schedule when item.ScheduleAnchorDate.HasValue:
                entity.Schedule = new DomainEntities.PlannedItemSchedule
                {
                    PlannedItemId = id,
                    Frequency = item.ScheduleFrequency ?? ScheduleFrequency.Monthly,
                    AnchorDate = item.ScheduleAnchorDate.Value,
                    Interval = item.ScheduleInterval ?? 1,
                    DayOfMonth = item.ScheduleDayOfMonth,
                    EndDate = item.ScheduleEndDate
                };
                break;

            case PlannedItemDateMode.FlexibleWindow when item.WindowStartDate.HasValue && item.WindowEndDate.HasValue:
                entity.FlexibleWindow = new DomainEntities.PlannedItemFlexibleWindow
                {
                    PlannedItemId = id,
                    StartDate = item.WindowStartDate.Value,
                    EndDate = item.WindowEndDate.Value,
                    AllocationMode = item.AllocationMode ?? AllocationMode.EvenlySpread
                };
                break;
        }

        return entity;
    }
}
