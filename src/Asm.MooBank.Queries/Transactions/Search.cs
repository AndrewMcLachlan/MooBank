using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Transactions;

public record Search(Guid AccountId, DateOnly StartDate, TransactionType TransactionType, IEnumerable<int> TagIds) : IQuery<IEnumerable<Transaction>>;
