using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("UpdateAccount")]
public record Update(LogicalAccount Account) : ICommand<LogicalAccount>;

internal class UpdateHandler(IUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Update, LogicalAccount>
{
    public async ValueTask<LogicalAccount> Handle(Update command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var account);

        if (account.GroupId != null)
        {
            security.AssertGroupPermission(account.GroupId.Value);
        }

        var entity = await accountRepository.Get(account.Id, new AccountDetailsSpecification(), cancellationToken);

        entity.Name = account.Name;
        entity.Description = account.Description;
        entity.SetController(account.Controller);
        entity.SetGroup(account.GroupId, user.Id);
        entity.AccountType = account.AccountType;
        entity.ShareWithFamily = account.ShareWithFamily;
        entity.IncludeInBudget = account.IncludeInBudget;

        

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
