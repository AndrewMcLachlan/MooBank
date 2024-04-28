using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding(Guid id) : Instrument.Instrument(id)
{
    public StockSymbol Symbol { get; set; } = null!;

    public int Quantity { get; set; }

    [Precision(12, 4)]
    public decimal CurrentPrice { get; set; }

    [Precision(12, 4)]
    public decimal CurrentValue { get; set; }

    [Precision(12, 4)]
    public decimal GainLoss { get; set; }

    public new DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.Now;

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
