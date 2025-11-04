using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;

[DisplayName("UpdateInstitutionAccount")]
public record Update(Guid InstrumentId, Models.Account.InstitutionAccount Account) : ICommand<Models.Account.InstitutionAccount>;

internal class UpdateHandler(IUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository) : ICommandHandler<Update, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Update command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var instrumentId, out var account);

        var logicalAccount = await accountRepository.Get(instrumentId, new AccountDetailsSpecification(), cancellationToken);
        var entity = logicalAccount.InstitutionAccounts.FirstOrDefault(a => a.Id == account.Id)
            ?? throw new NotFoundException($"Institution account with ID {account.Id} not found in logical account {instrumentId}");

        if (logicalAccount.Controller == Controller.Import)
        {
            _ = await accountRepository.GetImporterType(account.ImporterTypeId ?? throw new InvalidOperationException("Import account without importer type"), cancellationToken) ?? throw new NotFoundException("Unknown importer type ID " + account.ImporterTypeId);

            entity.ImporterTypeId = account.ImporterTypeId!.Value;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
