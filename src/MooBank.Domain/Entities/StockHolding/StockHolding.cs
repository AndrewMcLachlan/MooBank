using System.Diagnostics.CodeAnalysis;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.StockHolding;

[AggregateRoot]
public class StockHolding([DisallowNull] Guid id) : Instrument.Instrument(id)
{
    public static StockHolding Create(string name, string description, string symbol, bool shareWithFamily, decimal currentPrice)
    {
        var stockHolding = new StockHolding(Guid.Empty)
        {
            Name = name,
            Description = description,
            Symbol = symbol,
            ShareWithFamily = shareWithFamily,
            CurrentPrice = currentPrice,
            Controller = Controller.Manual,
        };

        stockHolding.MarkCreated();

        return stockHolding;
    }

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

    public void Update(string name, string description, bool shareWithFamily, decimal currentPrice)
    {
        Name = name;
        Description = description;
        ShareWithFamily = shareWithFamily;
        CurrentPrice = currentPrice;

        MarkUpdated();
    }
}
