namespace Asm.MooBank.Models;
public partial record TransactionOffset
{

}

public static class TransactionOffsetExtensions
{
    public static TransactionOffset ToOffsetByModel(this Domain.Entities.Transactions.TransactionOffset transactionOffset) =>
    new()
    {
        Transaction = transactionOffset.OffsetByTransaction.ToSimpleModel(),
        Amount = transactionOffset.Amount,
    };

    public static TransactionOffset ToOffsetModel(this Domain.Entities.Transactions.TransactionOffset transactionOffset) =>
new()
{
    Transaction = transactionOffset.Transaction.ToSimpleModel(),
    Amount = transactionOffset.Amount,
};
}
