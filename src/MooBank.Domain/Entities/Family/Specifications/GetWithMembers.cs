using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Family.Specifications;

public class GetWithMembers : ISpecification<Family>
{
    public IQueryable<Family> Apply(IQueryable<Family> query) =>
        query.Include(f => f.AccountHolders);
}
