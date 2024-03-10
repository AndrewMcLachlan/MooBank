using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.AccountHolder.Specifications;
public class GetWithCards : ISpecification<AccountHolder>
{
    public IQueryable<AccountHolder> Apply(IQueryable<AccountHolder> query) =>
        query.Include(q => q.Cards);

}
