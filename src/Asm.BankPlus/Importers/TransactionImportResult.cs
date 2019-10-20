using System.Collections.Generic;

namespace Asm.BankPlus.Models
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
