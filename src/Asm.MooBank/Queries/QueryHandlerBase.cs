using Asm.MooBank.Models;

namespace Asm.MooBank.Queries;

public abstract class QueryHandlerBase
{
    protected AccountHolder AccountHolder { get; }

    public QueryHandlerBase(AccountHolder accountHolder)
    {
        AccountHolder = accountHolder;
    }
}