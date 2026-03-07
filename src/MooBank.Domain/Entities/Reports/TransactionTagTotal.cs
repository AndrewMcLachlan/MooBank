namespace Asm.MooBank.Domain.Entities.Reports;

public class TransactionTagTotal
{
    public int TagId { get; set; }

    public string TagName { get; set; } = String.Empty;

    public decimal GrossAmount { get; set; }

    public decimal NetAmount { get; set; }

    public bool HasChildren { get; set; }
}

