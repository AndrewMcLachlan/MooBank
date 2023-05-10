using Asm.Cqrs.Queries;

namespace Asm.MooBank.Queries.AccountGroup;

public record GetAccountGroup(Guid Id) : IQuery<Models.AccountGroup>;
