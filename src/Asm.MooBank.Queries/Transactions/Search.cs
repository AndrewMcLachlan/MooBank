namespace Asm.MooBank.Models.Queries.Transactions;

public record Search(Guid AccountId, DateOnly StartDate, TransactionType TransactionType, IEnumerable<int> TagIds) : IQuery<IEnumerable<Transaction>>;
