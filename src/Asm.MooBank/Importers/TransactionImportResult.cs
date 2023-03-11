using System.Collections.Generic;
using Asm.MooBank.Models;

namespace Asm.MooBank.Importers
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
