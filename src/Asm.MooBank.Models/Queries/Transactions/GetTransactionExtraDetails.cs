namespace Asm.MooBank.Models.Queries.Transactions;

public abstract record GetTransactionExtraDetails(PagedResult<Transaction> Rransactions) : IQuery<PagedResult<Transaction>>;
