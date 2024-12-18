using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Queries.VirtualAccounts;

public record Get(Guid InstrumentId, Guid VirtualInstrumentId) : IQuery<VirtualInstrument>;

internal class GetHandler(IQueryable<Domain.Entities.Instrument.Instrument> accounts, ICurrencyConverter currencyConverter) : IQueryHandler<Get, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(Get request, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(va => va.RecurringTransactions).SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount) throw new InvalidOperationException("Virtual accounts are only available for institution accounts.");

        var virtualAccount = institutionAccount.VirtualInstruments.SingleOrDefault(va => va.Id == request.VirtualInstrumentId) ?? throw new NotFoundException();

        return virtualAccount.ToModel(currencyConverter);

    }
}
