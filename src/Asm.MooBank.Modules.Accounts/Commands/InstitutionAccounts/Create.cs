using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Modules.Accounts.Models.InstitutionAccount;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;

[DisplayName("CreateInstitutionAccount")]
public record Create(Guid InstrumentId, CreateInstitutionAccount InstitutionAccount) : InstrumentIdCommand(InstrumentId), ICommand<Models.Account.InstitutionAccount>;

internal class CreateHandler(ILogicalAccountRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        var logicalAccount = await accountRepository.Get(command.InstrumentId, new AccountDetailsSpecification(), cancellationToken);

        var entity = new Domain.Entities.Account.InstitutionAccount
        {
            InstitutionId = command.InstitutionAccount.InstitutionId,
            ImporterTypeId = command.InstitutionAccount.ImporterTypeId,
            Name = command.InstitutionAccount.Name,
            OpenedDate = command.InstitutionAccount.OpenedDate,
        };

        logicalAccount.AddInstitutionAccount(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
