using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

[AggregateRoot]
[PrimaryKey(nameof(Id))]
public class ForecastPlan(Guid id) : KeyedEntity<Guid>(id)
{
    public ForecastPlan() : this(Guid.Empty) { }

    public Guid FamilyId { get; set; }

    [ForeignKey(nameof(FamilyId))]
    public virtual Family.Family Family { get; set; } = null!;

    [MaxLength(200)]
    public required string Name { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public AccountScopeMode AccountScopeMode { get; set; }

    public StartingBalanceMode StartingBalanceMode { get; set; }

    [Precision(18, 2)]
    public decimal? StartingBalanceAmount { get; set; }

    [MaxLength(3)]
    public string? CurrencyCode { get; set; }

    [Column("IncomeStrategy")]
    public string? IncomeStrategySerialized { get; set; }

    [Column("OutgoingStrategy")]
    public string? OutgoingStrategySerialized { get; set; }

    [Column("Assumptions")]
    public string? AssumptionsSerialized { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public virtual ICollection<ForecastPlanAccount> Accounts { get; set; } = new HashSet<ForecastPlanAccount>();

    public virtual ICollection<ForecastPlannedItem> PlannedItems { get; set; } = new HashSet<ForecastPlannedItem>();

    public ForecastPlannedItem AddPlannedItem(ForecastPlannedItem item)
    {
        item.ForecastPlanId = Id;
        PlannedItems.Add(item);
        UpdatedUtc = DateTime.UtcNow;
        return item;
    }

    public void RemovePlannedItem(Guid itemId)
    {
        var item = PlannedItems.SingleOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            PlannedItems.Remove(item);
            UpdatedUtc = DateTime.UtcNow;
        }
    }

    public void SetAccounts(IEnumerable<Guid> instrumentIds)
    {
        Accounts.Clear();
        foreach (var instrumentId in instrumentIds)
        {
            Accounts.Add(new ForecastPlanAccount
            {
                ForecastPlanId = Id,
                InstrumentId = instrumentId
            });
        }
        UpdatedUtc = DateTime.UtcNow;
    }

    public void Archive()
    {
        IsArchived = true;
        UpdatedUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsArchived = false;
        UpdatedUtc = DateTime.UtcNow;
    }
}

public enum AccountScopeMode : byte
{
    AllAccounts = 0,
    SelectedAccounts = 1
}

public enum StartingBalanceMode : byte
{
    CalculatedCurrent = 0,
    ManualAmount = 1
}
