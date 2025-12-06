using System.ComponentModel;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using ModelContextProtocol.Server;
using PagedResult = Asm.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.McpTools;

[McpServerToolType]
public class TransactionTools(IQueryDispatcher queryDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-transactions", ReadOnly = true, Title = "Get Transactions")]
    [Description("Retrieves a paged list of transactions based on the provided filter criteria.")]
    public ValueTask<PagedResult> GetTransactions([Description("The filter, sorting and paging criteria")]Get criteria)
    {
        return queryDispatcher.Dispatch(criteria);
    }
}
