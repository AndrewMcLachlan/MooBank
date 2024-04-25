using System.ComponentModel.DataAnnotations.Schema;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding(Guid id) : Account.Instrument(id)
{
    public StockSymbol Symbol { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal CurrentPrice { get; set; }

    public decimal CurrentValue { get; set; }

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

    public override Group.Group? GetAccountGroup(Guid accountHolderId) =>
        base.GetAccountGroup(accountHolderId) ??
        ValidAccountViewers.Where(a => a.UserId == accountHolderId).Select(aah => aah.Group).SingleOrDefault();
}
