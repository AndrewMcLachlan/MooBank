using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;

[DisplayName("CloseInstitutionAccount")]
public record Close(Guid InstrumentId, Guid Id) : InstrumentIdCommand(InstrumentId), ICommand<Models.Account.InstitutionAccount>;

internal class CloseHandler(ILogicalAccountRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Close, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Close command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var instrumentId, out var Id);

        var logicalAccount = await accountRepository.Get(instrumentId, new AccountDetailsSpecification(), cancellationToken);
        var entity = logicalAccount.InstitutionAccounts.FirstOrDefault(a => a.Id == Id)
            ?? throw new NotFoundException($"Institution account with ID {Id} not found in logical account {instrumentId}");

        // TODO update with ASM and .NET 10
        entity.ClosedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
