namespace Asm.MooBank.Models.Ing;

public partial record TransactionExtra
{
    public Guid TransactionId { get; set; }

    public string Description { get; set; }

    public string PurchaseType { get; set; }

    public int ReceiptNumber { get; set; }

    public string Location { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string Reference { get; set; }
}
