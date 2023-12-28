using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class VirtualAccountSpecification : ISpecification<Account>
{
    public IQueryable<Account> Apply(IQueryable<Account> query) =>
        query.Include(a => a.VirtualAccounts).ThenInclude(a => a.RecurringTransactions);
}
