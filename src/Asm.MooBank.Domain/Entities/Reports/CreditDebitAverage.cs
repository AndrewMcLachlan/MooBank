namespace Asm.MooBank.Domain.Entities.Reports;

public class CreditDebitAverage
{
    public TransactionFilterType TransactionType { get; set; }

    public decimal Average { get; set; }
}
