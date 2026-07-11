using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Asset;

[AggregateRoot]
public class Asset(Guid id) : Instrument.Instrument(id)
{
    public Asset() : this(Guid.Empty)
    {
    }

    public static Asset Create(string name, string description, decimal value, bool shareWithFamily, decimal? purchasePrice)
    {
        var asset = new Asset(Guid.Empty)
        {
            Name = name,
            Description = description,
            Value = value,
            ShareWithFamily = shareWithFamily,
            PurchasePrice = purchasePrice,
        };

        asset.MarkCreated();

        return asset;
    }

    [Precision(12, 4)]
    public decimal? PurchasePrice { get; set; }

    [Precision(12, 4)]
    public decimal Value { get; set; }

    public void Update(string name, string description, decimal value, bool shareWithFamily, decimal? purchasePrice)
    {
        Name = name;
        Description = description;
        Value = value;
        ShareWithFamily = shareWithFamily;
        PurchasePrice = purchasePrice;

        MarkUpdated();
    }
}
