using Asm.Domain;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;
public class RecurringTransactionSpecification : ISpecification<Instrument>
{
    public IQueryable<Instrument> Apply(IQueryable<Instrument> query) =>
        query.Include(a => a.VirtualAccounts).ThenInclude(v => v.RecurringTransactions);
}
