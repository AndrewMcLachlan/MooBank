using Asm.MooBank.Commands;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.InstitutionAccount;

public record Update(Models.Account.InstitutionAccount Account) : ICommand<Models.Account.InstitutionAccount>;

internal class UpdateHandler(IUnitOfWork unitOfWork, IInstitutionAccountRepository accountRepository, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Account.InstitutionAccount>
{
    public async ValueTask<Models.Account.InstitutionAccount> Handle(Update request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var account);

        Security.AssertAccountPermission(account.Id);

        var entity = await accountRepository.Get(account.Id, cancellationToken);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.SetAccountGroup(account.AccountGroupId, AccountHolder.Id);
        entity.AccountType = account.AccountType;
        entity.ShareWithFamily = account.ShareWithFamily;
        entity.InstitutionId = account.InstitutionId;
        entity.IncludeInBudget = account.IncludeInBudget;

        if (account.Controller != entity.AccountController)
        {
            entity.AccountController = account.Controller;
            if (account.Controller == AccountController.Import)
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

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
