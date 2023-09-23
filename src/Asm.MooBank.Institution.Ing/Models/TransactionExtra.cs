namespace Asm.MooBank.Institution.Ing.Models;

public partial class TransactionExtra
{
    public string? PurchaseType { get; set; }

    public int? ReceiptNumber { get; set; }

    public DateOnly ProcessedDate { get; set; }

}
