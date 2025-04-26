using Asm.MooBank.Models;

namespace Asm.MooBank.Institution.Ing.Models;

internal record ParsedTransaction
{
    public Guid TransactionId { get; set; }

    public string? Description { get; set; }

    public string? PurchaseType { get; set; }

    public int? ReceiptNumber { get; set; }

    public string? Location { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string? Reference { get; set; }

    public short? Last4Digits { get; set; }

    public TransactionSubType? TransactionSubType { get; set; }
}
