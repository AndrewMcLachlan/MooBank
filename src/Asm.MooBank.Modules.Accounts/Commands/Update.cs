using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("UpdateAccount")]
public record Update(InstitutionAccount Account) : ICommand<InstitutionAccount>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IInstitutionAccountRepository accountRepository, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Update, InstitutionAccount>
{
    public async ValueTask<InstitutionAccount> Handle(Update command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var account);

        if (account.GroupId != null)
        {
            security.AssertGroupPermission(account.GroupId.Value);
        }

        var entity = await accountRepository.Get(account.Id, new AccountDetailsSpecification(), cancellationToken);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.SetGroup(account.GroupId, user.Id);
        entity.AccountType = account.AccountType;
        entity.ShareWithFamily = account.ShareWithFamily;
        entity.InstitutionId = account.InstitutionId;
        entity.IncludeInBudget = account.IncludeInBudget;

        if (account.Controller != entity.Controller)
        {
            entity.Controller = account.Controller;
            if (account.Controller == Controller.Import)
            {
                _ = await accountRepository.GetImporterType(account.ImporterTypeId ?? throw new InvalidOperationException("Import account without importer type"), cancellationToken) ?? throw new NotFoundException("Unknown importer type ID " + account.ImporterTypeId);

                entity.ImportAccount = new Domain.Entities.Account.ImportAccount
                {
                    AccountId = entity.Id,
                    ImporterTypeId = account.ImporterTypeId!.Value,
                };
            }
            else if (entity.ImportAccount != null)
            {
                accountRepository.RemoveImportAccount(entity.ImportAccount);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
