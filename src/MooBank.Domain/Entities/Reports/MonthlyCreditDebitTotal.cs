using Asm.MooBank.Models;

namespace Asm.MooBank.Domain.Entities.Reports;

public class MonthlyCreditDebitTotal
{
    public DateOnly Month { get; set; }

    public TransactionFilterType TransactionType { get; set; }

    public decimal Total { get; set; }
}
