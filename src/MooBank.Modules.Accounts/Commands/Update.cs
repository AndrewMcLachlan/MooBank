using System.ComponentModel;
using Asm.MooBank.Audit;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("UpdateAccount")]
public record Update(LogicalAccount Account) : ICommand<LogicalAccount>;

internal class UpdateHandler(IAuditingUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Update, LogicalAccount>
{
    public async ValueTask<LogicalAccount> Handle(Update command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var account);

        if (account.GroupId != null)
        {
            await security.AssertGroupPermission(account.GroupId.Value);
        }

        var entity = await accountRepository.Get(account.Id, new AccountDetailsSpecification(), cancellationToken);

        entity.Update(account.Name, account.Description, account.Controller, account.AccountType, account.ShareWithFamily, account.IncludeInBudget);
        entity.SetGroup(account.GroupId, user.Id);

        accountRepository.Update(entity);

        await unitOfWork.SaveChangesAsync("Updated", "Account", entity.Id, cancellationToken);

        return await entity.ToModel(currencyConverter, cancellationToken);
    }
}
