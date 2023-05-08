using Asm.Cqrs.Queries;

namespace Asm.MooBank.Models.Queries.AccountGroup;

public class GetAccountGroups : IQuery<IEnumerable<Models.AccountGroup>>
{
}
