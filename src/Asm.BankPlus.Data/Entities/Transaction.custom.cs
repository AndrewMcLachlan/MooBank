namespace Asm.BankPlus.Data.Entities
{
    public partial class Transaction
    {
        public static implicit operator Models.Transaction(Transaction transaction)
        {
            return new Models.Transaction
            {
                Id = transaction.TransactionId,
                Reference = transaction.TransactionReference,
                Amount = transaction.Amount,
                TransactionTime = transaction.TransactionTime,
                TransactionType = transaction.TransactionType,
                AccountId = transaction.AccountId,
                Description = transaction.Description,
            };
        }

        public static implicit operator Transaction(Models.Transaction transaction)
        {
            return new Transaction
            {
                TransactionId = transaction.Id,
                TransactionReference = transaction.Reference,
                Amount = transaction.Amount,
                TransactionTime = transaction.TransactionTime,
                TransactionType = transaction.TransactionType,
                AccountId = transaction.AccountId,
                Description = transaction.Description,
            };
        }
    }
}
