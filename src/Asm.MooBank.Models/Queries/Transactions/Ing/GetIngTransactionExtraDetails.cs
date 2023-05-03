namespace Asm.MooBank.Models.Queries.Transactions.Ing;

public record GetIngTransactionExtraDetails(PagedResult<Transaction> Transactions) : GetTransactionExtraDetails(Transactions);
