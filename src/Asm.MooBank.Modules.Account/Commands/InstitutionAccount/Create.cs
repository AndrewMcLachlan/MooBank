using Asm.MooBank.Commands;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccount;

public record Create(Models.Account.InstitutionAccount Account) : ICommand<Models.Account.InstitutionAccount>;

internal class CreateHandler(IInstitutionAccountRepository institutionAccountRepository, IUnitOfWork unitOfWork, User accountHolder, ICurrencyConverter currencyConverter, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Account.InstitutionAccount>
{
    private readonly IInstitutionAccountRepository _accountRepository = institutionAccountRepository;

    public async ValueTask<Models.Account.InstitutionAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        var account = command.Account;

        if (account.GroupId != null)
        {
            Security.AssertAccountGroupPermission(account.GroupId.Value);
        }

        var entity = account.ToEntity();

        entity.SetAccountHolder(AccountHolder.Id);
        entity.SetAccountGroup(account.GroupId, AccountHolder.Id);

        _accountRepository.Add(entity);

        if (account.ImporterTypeId != null)
        {
            entity.ImportAccount = new Domain.Entities.Account.ImportAccount
            {
                ImporterTypeId = account.ImporterTypeId.Value,
            };
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
