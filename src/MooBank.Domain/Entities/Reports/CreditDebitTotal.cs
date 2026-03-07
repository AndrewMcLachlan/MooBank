using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Reports;

public class CreditDebitTotal
{
    public TransactionFilterType TransactionType { get; set; }

    public decimal Total { get; set; }
}
