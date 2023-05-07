namespace Asm.MooBank.Models.Ing;

public partial record TransactionExtra
{
    public static explicit operator Models.Ing.TransactionExtra(Domain.Entities.Ing.TransactionExtra entity)
    {
        return new Models.Ing.TransactionExtra
        {
            Description = entity.Description,
            Location = entity.Location,
            PurchaseDate = entity.PurchaseDate,
            PurchaseType = entity.PurchaseType,
            ReceiptNumber = entity.ReceiptNumber,
            Reference = entity.Reference,
            TransactionId = entity.TransactionId,
        };
    }

    public static explicit operator Domain.Entities.Ing.TransactionExtra(Models.Ing.TransactionExtra entity)
    {
        return new Domain.Entities.Ing.TransactionExtra
        {
            Description = entity.Description,
            Location = entity.Location,
            PurchaseDate = entity.PurchaseDate,
            PurchaseType = entity.PurchaseType,
            ReceiptNumber = entity.ReceiptNumber,
            Reference = entity.Reference,
            TransactionId = entity.TransactionId,
        };
    }
}

public static class TransactionExtraExtensions
{
    public static TransactionExtra ToModel(this Domain.Entities.Ing.TransactionExtra entity, IDictionary<short, string?> cardNames)
    {
        return new TransactionExtra
        {
            Description = entity.Description,
            Location = entity.Location,
            PurchaseDate = entity.PurchaseDate,
            PurchaseType = entity.PurchaseType,
            ReceiptNumber = entity.ReceiptNumber,
            Reference = entity.Reference,
            TransactionId = entity.TransactionId,
            Who = entity.Last4Digits != null && cardNames.ContainsKey(entity.Last4Digits.Value) ? cardNames[entity.Last4Digits.Value] : null,
        };
    }
}