using Asm.Cqrs.Queries;

namespace Asm.MooBank.Queries.AccountGroup;

public class GetAccountGroups : IQuery<IEnumerable<Models.AccountGroup>>
{
}
