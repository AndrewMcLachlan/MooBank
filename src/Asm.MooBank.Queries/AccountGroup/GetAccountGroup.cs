using Asm.Cqrs.Queries;

namespace Asm.MooBank.Models.Queries.AccountGroup;

public record GetAccountGroup(Guid Id) : IQuery<Models.AccountGroup>;
