using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Instrument.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using DomainLogicalAccount = Asm.MooBank.Domain.Entities.Account.LogicalAccount;

namespace Asm.MooBank.Modules.Accounts.Queries;

public record GetAll() : IQuery<IEnumerable<LogicalAccount>>;

internal class GetAllHandler(IQueryable<DomainLogicalAccount> logicalAccounts, User user, ICurrencyConverter currencyConverter) : IQueryHandler<GetAll, IEnumerable<LogicalAccount>>
{
    public async ValueTask<IEnumerable<LogicalAccount>> Handle(GetAll request, CancellationToken cancellationToken)
    {
        var accounts = (await logicalAccounts
            .Apply(new OpenAccessibleSpecification<DomainLogicalAccount>(user.Id, user.FamilyId))
            .ToModelAsync(currencyConverter, cancellationToken)).ToList();

        var primary = accounts.SingleOrDefault(a => a.Id == user.PrimaryAccountId);

        primary?.IsPrimary = true;

        return accounts;
    }
}
