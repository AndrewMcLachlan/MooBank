using System.ComponentModel;
using Asm.MooBank.Modules.Transactions.Queries.Transactions;
using Asm.MooBank.Security;
using ModelContextProtocol.Server;
using PagedResult = Asm.PagedResult<Asm.MooBank.Models.Transaction>;

namespace Asm.MooBank.Modules.Transactions.McpTools;

[McpServerToolType]
public class TransactionTools(IQueryDispatcher queryDispatcher, ISecurity security)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-transactions", ReadOnly = true, Title = "Get Transactions")]
    [Description("Retrieves a paged list of transactions based on the provided filter criteria.")]
    public async ValueTask<PagedResult> GetTransactions([Description("The filter, sorting and paging criteria")] Get criteria)
    {
        await security.AssertInstrumentViewer(criteria.InstrumentId);

        return await queryDispatcher.Dispatch(criteria);
    }
}
