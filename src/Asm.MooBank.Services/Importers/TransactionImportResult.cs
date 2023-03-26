using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Importers;

public class TransactionImportResult
{
    public IEnumerable<Transaction> Transactions { get; init; }

    public decimal EndBalance { get; init; }

    public TransactionImportResult(IEnumerable<Transaction> transactions, decimal endBalance)
    {
        Transactions = transactions;
        EndBalance = endBalance;
    }
}
