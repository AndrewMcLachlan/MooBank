namespace Asm.MooBank.Services.Queries;

internal abstract class QueryHandlerBase
{
    protected ISecurityRepository Security { get; }

    public QueryHandlerBase(ISecurityRepository security)
    {
        Security = security;
    }
}