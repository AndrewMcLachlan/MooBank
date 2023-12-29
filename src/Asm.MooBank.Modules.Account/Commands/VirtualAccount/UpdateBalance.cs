﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.VirtualAccount;

public record UpdateBalance(Guid AccountId, Guid VirtualAccountId, decimal Balance) : ICommand<Models.Account.VirtualAccount>;

internal class UpdateBalanceHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ISecurity security, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<UpdateBalance, Models.Account.VirtualAccount>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Account.VirtualAccount> Handle(UpdateBalance request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        var entity = account.VirtualAccounts.SingleOrDefault(va => va.Id == request.VirtualAccountId) ?? throw new NotFoundException();

        var amount = entity.Balance - request.Balance;

        //TODO: Should be done via domain event
        _transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = entity,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        entity.Balance = request.Balance;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
