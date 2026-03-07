using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Account.Specifications;

public class VirtualAccountSpecification : ISpecification<Instrument.Instrument>
{
    public IQueryable<Instrument.Instrument> Apply(IQueryable<Instrument.Instrument> query) =>
        query.Include(a => a.VirtualInstruments).ThenInclude(a => a.RecurringTransactions);
}
