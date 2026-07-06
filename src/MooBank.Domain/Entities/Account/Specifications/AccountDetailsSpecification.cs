using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class AccountDetailsSpecification : ISpecification<LogicalAccount>
{
    public IQueryable<LogicalAccount> Apply(IQueryable<LogicalAccount> query) =>
        query.Include(a => a.Owners).ThenInclude(o => o.User)
             .Include(a => a.Owners).ThenInclude(o => o.Group)
             .Include(a => a.Viewers).ThenInclude(v => v.User)
             .Include(a => a.Viewers).ThenInclude(v => v.Group)
             .Include(a => a.InstitutionAccounts)
             .Include(a => a.TagPurposes);
}
