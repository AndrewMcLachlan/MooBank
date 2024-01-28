using Asm.MooBank.Models;

namespace Asm.MooBank.Queries;

public abstract class QueryHandlerBase(AccountHolder accountHolder)
{
    protected AccountHolder AccountHolder { get; } = accountHolder;
}
