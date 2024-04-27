using Asm.MooBank.Models;

namespace Asm.MooBank.Queries;

public abstract class QueryHandlerBase(User accountHolder)
{
    protected User AccountHolder { get; } = accountHolder;
}
