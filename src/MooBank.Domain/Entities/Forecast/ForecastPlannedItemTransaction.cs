using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Forecast;

/// <summary>
/// A payment the plan's author has said belongs to a planned item.
/// </summary>
/// <remarks>
/// Tags alone cannot identify an item. A tag is a category and a planned item is a specific project,
/// so one "Home Improvements" tag covers the solar panels, the fence and the renovation, and no rule
/// over tags and dates can tell which payment belongs to which. The author can, and this is where
/// they say so.
///
/// The tag still earns its keep: it narrows the candidates offered for linking to spending that
/// carried the item's tag around the item's own date, rather than every transaction on the plan.
///
/// <see cref="ForecastPlanId"/> is carried here so a payment can be claimed by only one item within
/// a plan, which the unique index enforces. A payment covering two items — a single school fees
/// invoice for two children — is a sign the two should be one item.
/// </remarks>
[PrimaryKey(nameof(Id))]
public class ForecastPlannedItemTransaction(Guid id) : KeyedEntity<Guid>(id)
{
    public ForecastPlannedItemTransaction() : this(Guid.Empty) { }

    public Guid PlannedItemId { get; set; }

    [ForeignKey(nameof(PlannedItemId))]
    public virtual ForecastPlannedItem PlannedItem { get; set; } = null!;

    /// <summary>
    /// The plan the item belongs to, denormalised so the database can enforce one item per payment.
    /// </summary>
    public Guid ForecastPlanId { get; set; }

    public Guid TransactionId { get; set; }

    [ForeignKey(nameof(TransactionId))]
    public virtual Transactions.Transaction Transaction { get; set; } = null!;
}
