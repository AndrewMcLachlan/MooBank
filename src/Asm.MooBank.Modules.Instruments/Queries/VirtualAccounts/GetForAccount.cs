using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Queries.VirtualAccounts;
public record GetForAccount(Guid InstrumentId) : IQuery<IEnumerable<VirtualInstrument>>;

internal class GetForAccountHandler(IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, ICurrencyConverter currencyConverter) : IQueryHandler<GetForAccount, IEnumerable<VirtualInstrument>>
{

    public async ValueTask<IEnumerable<VirtualInstrument>> Handle(GetForAccount request, CancellationToken cancellationToken)
    {
        var account = await accounts.Include(a => a.VirtualInstruments).SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken);

        return account != null ? account.VirtualInstruments.ToModel(currencyConverter) : throw new NotFoundException("Account not found");
    }
}
