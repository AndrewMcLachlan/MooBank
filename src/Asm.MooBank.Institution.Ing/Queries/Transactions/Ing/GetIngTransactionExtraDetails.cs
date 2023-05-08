using Asm.MooBank.Models;
using Asm.MooBank.Models.Queries.Transactions;

namespace Asm.MooBank.Institution.Ing.Queries.Transactions;

public record GetIngTransactionExtraDetails(Guid AccountId, PagedResult<Transaction> Transactions) : GetTransactionExtraDetails(Transactions);
