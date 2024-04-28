using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.User.Specifications;
public class GetWithCards : ISpecification<User>
{
    public IQueryable<User> Apply(IQueryable<User> query) =>
        query.Include(q => q.Cards);

}
