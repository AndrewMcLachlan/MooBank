using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Queries.VirtualInstruments;

public record GetForInstrument(Guid InstrumentId) : IQuery<IEnumerable<VirtualInstrument>>;

internal class GetForInstrumentHandler(IQueryable<Domain.Entities.Account.LogicalAccount> accounts, ICurrencyConverter currencyConverter) : IQueryHandler<GetForInstrument, IEnumerable<VirtualInstrument>>
{

    public async ValueTask<IEnumerable<VirtualInstrument>> Handle(GetForInstrument request, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.VirtualInstruments).SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken);

        return account != null ? await account.VirtualInstruments.ToModel(currencyConverter, cancellationToken) : throw new NotFoundException("Account not found");
    }
}
