using Asm.MooBank.Models;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Commands.Account;

public record Update(InstitutionAccount Account) : ICommand<InstitutionAccount>;

internal class UpdateHandler : CommandHandlerBase, ICommandHandler<Update, InstitutionAccount>
{
    private readonly IInstitutionAccountRepository _accountRepository;

    public UpdateHandler(IUnitOfWork unitOfWork, IInstitutionAccountRepository accountRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _accountRepository = accountRepository;
    }

    public async Task<InstitutionAccount> Handle(Update request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out var account);

        Security.AssertAccountPermission(account.Id);

        var entity = await _accountRepository.Get(account.Id, cancellationToken);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.SetAccountGroup(account.AccountGroupId, AccountHolder.Id);
        entity.AccountType = account.AccountType;
        entity.ShareWithFamily = account.ShareWithFamily;
        entity.InstitutionId = account.InstitutionId;

        if (account.Controller != entity.AccountController)
        {
            entity.AccountController = account.Controller;
            if (account.Controller == AccountController.Import)
            {
                var importerType = await _accountRepository.GetImporterType(account.ImporterTypeId ?? throw new InvalidOperationException("Import account without importer type"), cancellationToken);

                if (importerType == null) throw new NotFoundException("Unknown importer type ID " + account.ImporterTypeId);

                entity.ImportAccount = new Domain.Entities.Account.ImportAccount
                {
                    AccountId = entity.AccountId,
                    ImporterTypeId = account.ImporterTypeId!.Value,
                };
            }
            else if (entity.ImportAccount != null)
            {
                _accountRepository.RemoveImportAccount(entity.ImportAccount);
            }
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
