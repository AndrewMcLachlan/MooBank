using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class VirtualAccountSpecification : ISpecification<Instrument>
{
    public IQueryable<Instrument> Apply(IQueryable<Instrument> query) =>
        query.Include(a => a.VirtualAccounts).ThenInclude(a => a.RecurringTransactions);
}
