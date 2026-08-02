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

    /// <summary>
    /// Records which payments belong to a planned item, replacing whatever was there before.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the item does not belong to this plan.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a payment already belongs to another item on the plan. A payment covering two
    /// items is a sign the two should be one item, and counting it against both would overstate the
    /// plan by its whole amount.
    /// </exception>
    public void SetPlannedItemTransactions(Guid plannedItemId, IEnumerable<Guid> transactionIds)
    {
        var item = PlannedItems.SingleOrDefault(i => i.Id == plannedItemId)
            ?? throw new NotFoundException("Planned item not found");

        var ids = transactionIds.Distinct().ToList();

        var takenElsewhere = PlannedItems
            .Where(i => i.Id != plannedItemId)
            .SelectMany(i => i.Transactions)
            .Where(t => ids.Contains(t.TransactionId))
            .Select(t => t.TransactionId)
            .ToList();

        if (takenElsewhere.Count > 0)
        {
            throw new InvalidOperationException("That payment already belongs to another planned item on this plan");
        }

        item.Transactions.Clear();

        foreach (var transactionId in ids)
        {
            item.Transactions.Add(new ForecastPlannedItemTransaction
            {
                PlannedItemId = item.Id,
                ForecastPlanId = Id,
                TransactionId = transactionId,
            });
        }

        UpdatedUtc = DateTime.UtcNow;
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
