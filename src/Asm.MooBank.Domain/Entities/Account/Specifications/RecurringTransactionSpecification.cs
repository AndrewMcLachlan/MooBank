using Asm.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;
public class RecurringTransactionSpecification : ISpecification<Account>
{
    public IQueryable<Account> Apply(IQueryable<Account> query) =>
        query.Include(a => a.VirtualAccounts).ThenInclude(v => v.RecurringTransactions);
}
