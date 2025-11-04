using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccounts;

[DisplayName("CreateInstitutionAccount")]
public record Create() : ICommand<Models.Account.InstitutionAccount>
{
    public required Guid InstrumentId { get; init; }

    public required int InstitutionId { get; init; }

    public int? ImporterTypeId { get; init; }

}

internal class CreateHandler(ILogicalAccountRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        var logicalAccount = await accountRepository.Get(command.InstrumentId, new AccountDetailsSpecification(), cancellationToken);

        var entity = new Domain.Entities.Account.InstitutionAccount
        {
            InstitutionId = command.InstitutionId,
            ImporterTypeId = command.ImporterTypeId,
        };

        logicalAccount.AddInstitutionAccount(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
