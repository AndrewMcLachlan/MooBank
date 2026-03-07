using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Accounts.Queries.InstitutionAccounts;

public record Get(Guid InstrumentId, Guid Id) : IQuery<InstitutionAccount>;

internal class GetHandler(IQueryable<Domain.Entities.Account.LogicalAccount> accounts) : IQueryHandler<Get, InstitutionAccount>
{
    public async ValueTask<InstitutionAccount> Handle(Get request, CancellationToken cancellationToken)
    {
        var logicalAccount = await accounts.SingleOrDefaultAsync(a => a.Id == request.InstrumentId, cancellationToken) ?? throw new NotFoundException();

        var entity = logicalAccount.InstitutionAccounts.FirstOrDefault(a => a.Id == request.Id)
            ?? throw new NotFoundException($"Institution account with ID {request.Id} not found in logical account {request.InstrumentId}");

        return entity.ToModel();
    }
}
