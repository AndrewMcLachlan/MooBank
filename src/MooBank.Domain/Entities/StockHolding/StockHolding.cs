using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding([DisallowNull] Guid id) : Instrument.Instrument(id)
{
    public StockSymbolEntity Symbol { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int Quantity { get; set; }

    [Precision(12, 4)]
    public decimal CurrentPrice { get; set; }

    [Precision(12, 4)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal CurrentValue { get; set; }

    [Precision(12, 4)]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal GainLoss { get; set; }

    public ICollection<StockTransaction> Transactions { get; set; } = [];

    [NotMapped]
    public IEnumerable<InstrumentViewer> ValidAccountViewers
    {
        get
        {
            if (!ShareWithFamily) return [];
            var familyIds = base.Owners.Select(a => a.User.FamilyId).Distinct();
            return Viewers.Where(a => familyIds.Contains(a.User.FamilyId));
        }
    }

    public override Group.Group? GetGroup(Guid accountHolderId) =>
        base.GetGroup(accountHolderId) ??
        ValidAccountViewers.Where(a => a.UserId == accountHolderId).Select(aah => aah.Group).SingleOrDefault();
}
