using System.Collections.Generic;

namespace Asm.MooBank.Models
{
    public class TransactionImportResult
    {
        public IEnumerable<Transaction> Transactions { get; }

        public TransactionImportResult(IEnumerable<Transaction> transactions)
        {
            Transactions = transactions;
        }
    }
}
