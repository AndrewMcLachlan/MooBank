using Asm.MooBank.Commands;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.InstitutionAccount;

public record Create(Models.Account.InstitutionAccount Account, ImportAccount ImportAccount) : ICommand<Models.Account.InstitutionAccount>;

internal class CreateHandler(IInstitutionAccountRepository institutionAccountRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ICurrencyConverter currencyConverter, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Account.InstitutionAccount>
{
    private readonly IInstitutionAccountRepository _accountRepository = institutionAccountRepository;

    public async ValueTask<Models.Account.InstitutionAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        var account = command.Account;

        if (account.AccountGroupId != null)
        {
            Security.AssertAccountGroupPermission(account.AccountGroupId.Value);
        }

        var entity = account.ToEntity();

        entity.SetAccountHolder(AccountHolder.Id);
        entity.SetAccountGroup(account.AccountGroupId, AccountHolder.Id);

        _accountRepository.Add(entity);

        if (account.ImporterTypeId != null || command.ImportAccount != null)
        {
            entity.ImportAccount = new Domain.Entities.Account.ImportAccount
            {
                ImporterTypeId = account.ImporterTypeId ?? command.ImportAccount.ImporterTypeId,
            };
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
