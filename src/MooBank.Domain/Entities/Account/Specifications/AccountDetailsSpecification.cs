using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class AccountDetailsSpecification : ISpecification<LogicalAccount>
{
    public IQueryable<LogicalAccount> Apply(IQueryable<LogicalAccount> query) =>
        query.Include(a => a.Owners).Include(a => a.Viewers).Include(a => a.InstitutionAccounts);
}
