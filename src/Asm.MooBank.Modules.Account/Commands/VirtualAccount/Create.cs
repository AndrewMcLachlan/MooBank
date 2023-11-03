﻿using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;

namespace Asm.MooBank.Modules.Account.Commands.VirtualAccount;

public record Create(Guid AccountId, Models.Account.VirtualAccount VirtualAccount) : ICommand<Models.Account.VirtualAccount>;

internal class CreateHandler(Domain.Entities.Account.IAccountRepository accountRepository, Domain.Entities.Transactions.ITransactionRepository transactionRepository, ISecurity securityRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, Models.Account.VirtualAccount>
{
    private readonly Domain.Entities.Account.IAccountRepository _accountRepository = accountRepository;
    private readonly Domain.Entities.Transactions.ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ISecurity _security = securityRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Create request, CancellationToken cancellationToken)
    {
        _security.AssertAccountPermission(request.AccountId);
        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Cannot create virtual account on non-institution account.");
        }

        var entity = request.VirtualAccount.ToEntity(request.AccountId);

        // TODO: Should use domain events here.
        if (entity.Balance != 0)
        {
            _transactionRepository.Add(new Domain.Entities.Transactions.Transaction
            {
                Account = entity,
                Amount = entity.Balance,
                Description = "Initial balance",
                Source = "Web",
                TransactionTime = DateTime.Now,
                TransactionType = entity.Balance > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
            });
        }

        institutionAccount.VirtualAccounts.Add(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
