namespace Asm.MooBank.Models.Queries.Transactions.Ing;

public record GetIngTransactionExtraDetails(Guid AccountId, PagedResult<Transaction> Transactions) : GetTransactionExtraDetails(Transactions);
