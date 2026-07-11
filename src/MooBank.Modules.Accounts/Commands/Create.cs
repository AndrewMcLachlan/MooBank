using System.ComponentModel;
using Asm.MooBank.Audit;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("CreateAccount")]
public record Create() : ICommand<LogicalAccount>
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required int InstitutionId { get; init; }

    public required string Currency { get; init; }

    public required decimal Balance { get; init; }

    public DateOnly? OpenedDate { get; init; }

    public Guid? GroupId { get; init; }

    public AccountType AccountType { get; init; }

    public Controller Controller { get; init; }

    public bool IncludeInBudget { get; init; }

    public bool ShareWithFamily { get; init; }
}

internal class CreateHandler(ILogicalAccountRepository institutionAccountRepository, IAuditingUnitOfWork unitOfWork, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Create, LogicalAccount>
{
    private readonly ILogicalAccountRepository _accountRepository = institutionAccountRepository;

    public async ValueTask<LogicalAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            await security.AssertGroupPermission(command.GroupId.Value);
        }

        var openedDate = command.OpenedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var entity = Domain.Entities.Account.LogicalAccount.Create(
            command.Name,
            command.Description,
            command.Currency,
            command.AccountType,
            command.Controller,
            command.IncludeInBudget,
            command.ShareWithFamily,
            new Domain.Entities.Account.InstitutionAccount
            {
                Name = command.Name,
                OpenedDate = openedDate,
                InstitutionId = command.InstitutionId,
            },
            command.Balance,
            openedDate);

        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        _accountRepository.Add(entity);

        await unitOfWork.SaveChangesAsync("Created", "Account", entity.Id, cancellationToken);

        return await entity.ToModel(currencyConverter, cancellationToken);
    }
}
