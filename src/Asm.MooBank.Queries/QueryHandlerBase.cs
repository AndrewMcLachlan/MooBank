namespace Asm.MooBank.Services.Queries;

internal abstract class QueryHandlerBase
{
    protected ISecurity Security { get; }

    public QueryHandlerBase(ISecurity security)
    {
        Security = security;
    }
}