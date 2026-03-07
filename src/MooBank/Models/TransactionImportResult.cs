namespace Asm.MooBank.Models;

public class TransactionImportResult(IEnumerable<Domain.Entities.Transactions.Transaction> transactions, decimal? endBalance = null)
{
    public IEnumerable<Domain.Entities.Transactions.Transaction> Transactions { get; init; } = transactions;

    public decimal? EndBalance { get; init; } = endBalance;
}
