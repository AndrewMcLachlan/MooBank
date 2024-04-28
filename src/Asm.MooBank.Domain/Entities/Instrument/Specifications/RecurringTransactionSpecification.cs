using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Instrument.Specifications;

public class RecurringTransactionSpecification : ISpecification<Instrument>
{
    public IQueryable<Instrument> Apply(IQueryable<Instrument> query) =>
        query.Include(a => a.VirtualInstruments).ThenInclude(v => v.RecurringTransactions);
}
