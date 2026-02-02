#nullable enable
using Asm.MooBank.Models;
using Bogus;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;
using DomainPlannedItem = Asm.MooBank.Domain.Entities.Forecast.ForecastPlannedItem;
using ModelPlannedItem = Asm.MooBank.Modules.Forecast.Models.PlannedItem;

namespace Asm.MooBank.Modules.Forecast.Tests.Support;

internal static class TestEntities
{
    private static readonly Faker Faker = new();

    public static DomainForecastPlan CreateForecastPlan(
        Guid? id = null,
        string? name = null,
        Guid? familyId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        AccountScopeMode accountScopeMode = AccountScopeMode.AllAccounts,
        StartingBalanceMode startingBalanceMode = StartingBalanceMode.ManualAmount,
        decimal? startingBalanceAmount = null,
        string? currencyCode = "AUD",
        bool isArchived = false,
        IEnumerable<DomainPlannedItem>? plannedItems = null)
    {
        var planId = id ?? Guid.NewGuid();
        var plan = new DomainForecastPlan(planId)
        {
            Name = name ?? Faker.Lorem.Sentence(3),
            FamilyId = familyId ?? Guid.NewGuid(),
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.Today),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
            AccountScopeMode = accountScopeMode,
            StartingBalanceMode = startingBalanceMode,
            StartingBalanceAmount = startingBalanceAmount ?? 10000m,
            CurrencyCode = currencyCode,
            IsArchived = isArchived,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
        };

        if (plannedItems != null)
        {
            foreach (var item in plannedItems)
            {
                item.ForecastPlanId = planId;
                plan.PlannedItems.Add(item);
            }
        }

        return plan;
    }

    public static DomainPlannedItem CreatePlannedItem(
        Guid? id = null,
        Guid? planId = null,
        string? name = null,
        PlannedItemType itemType = PlannedItemType.Expense,
        decimal amount = 100m,
        int? tagId = null,
        Guid? virtualInstrumentId = null,
        bool isIncluded = true,
        PlannedItemDateMode dateMode = PlannedItemDateMode.FixedDate,
        DateOnly? fixedDate = null,
        string? notes = null)
    {
        var itemId = id ?? Guid.NewGuid();
        var item = new DomainPlannedItem(itemId)
        {
            ForecastPlanId = planId ?? Guid.NewGuid(),
            Name = name ?? Faker.Commerce.ProductName(),
            ItemType = itemType,
            Amount = amount,
            TagId = tagId,
            VirtualInstrumentId = virtualInstrumentId,
            IsIncluded = isIncluded,
            DateMode = dateMode,
            Notes = notes,
        };

        if (dateMode == PlannedItemDateMode.FixedDate)
        {
            item.FixedDate = new Asm.MooBank.Domain.Entities.Forecast.PlannedItemFixedDate
            {
                PlannedItemId = itemId,
                FixedDate = fixedDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            };
        }

        return item;
    }

    public static ModelPlannedItem CreatePlannedItemModel(
        Guid? id = null,
        string? name = null,
        PlannedItemType itemType = PlannedItemType.Expense,
        decimal amount = 100m,
        int? tagId = null,
        Guid? virtualInstrumentId = null,
        bool isIncluded = true,
        PlannedItemDateMode dateMode = PlannedItemDateMode.FixedDate,
        DateOnly? fixedDate = null,
        string? notes = null)
    {
        return new ModelPlannedItem
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? Faker.Commerce.ProductName(),
            ItemType = itemType,
            Amount = amount,
            TagId = tagId,
            VirtualInstrumentId = virtualInstrumentId,
            IsIncluded = isIncluded,
            DateMode = dateMode,
            FixedDate = fixedDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            Notes = notes,
        };
    }

    public static List<DomainForecastPlan> CreateSamplePlans(Guid? familyId = null)
    {
        var fid = familyId ?? Guid.NewGuid();
        return
        [
            CreateForecastPlan(name: "2024 Plan", familyId: fid),
            CreateForecastPlan(name: "2025 Plan", familyId: fid),
            CreateForecastPlan(name: "Archived Plan", familyId: fid, isArchived: true),
        ];
    }

    public static IQueryable<DomainForecastPlan> CreatePlanQueryable(IEnumerable<DomainForecastPlan> plans)
    {
        return QueryableHelper.CreateAsyncQueryable(plans);
    }

    public static IQueryable<DomainForecastPlan> CreatePlanQueryable(params DomainForecastPlan[] plans)
    {
        return CreatePlanQueryable(plans.AsEnumerable());
    }
}
