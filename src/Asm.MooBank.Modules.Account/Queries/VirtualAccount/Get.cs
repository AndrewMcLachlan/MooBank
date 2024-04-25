using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Account.Queries.VirtualAccount;

public record Get(Guid AccountId, Guid VirtualAccountId) : IQuery<Models.Account.VirtualInstrument>;

internal class GetHandler(IQueryable<Domain.Entities.Account.Instrument> accounts, ISecurity security, ICurrencyConverter currencyConverter) : IQueryHandler<Get, Models.Account.VirtualInstrument>
{
    public async ValueTask<Models.Account.VirtualInstrument> Handle(Get request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        var account = await accounts.Include(a => a.VirtualInstruments).ThenInclude(va => va.RecurringTransactions).SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken) ?? throw new NotFoundException();

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount) throw new InvalidOperationException("Virtual accounts are only available for institution accounts.");

        var virtualAccount = institutionAccount.VirtualInstruments.SingleOrDefault(va => va.Id == request.VirtualAccountId) ?? throw new NotFoundException();

        return virtualAccount.ToModel(currencyConverter);

    }
}
