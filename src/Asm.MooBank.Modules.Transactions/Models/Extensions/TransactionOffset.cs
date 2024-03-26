namespace Asm.MooBank.Modules.Transactions.Models.Extensions;

public static class TransactionOffsetExtensions
{
    public static TransactionOffsetBy ToOffsetByModel(this Domain.Entities.Transactions.TransactionOffset transactionOffset) =>
    new()
    {
        Transaction = transactionOffset.OffsetByTransaction.ToSimpleModel(),
        Amount = transactionOffset.Amount,
    };

    public static TransactionOffsetFor ToOffsetForModel(this Domain.Entities.Transactions.TransactionOffset transactionOffset) =>
    new()
    {
        Transaction = transactionOffset.TransactionSplit.Transaction.ToSimpleModel(),
        Amount = transactionOffset.Amount,
    };
}
