﻿using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record Create(Guid InstrumentId, decimal Amount, string Description, string? Reference, DateTimeOffset TransactionTime) : InstrumentIdCommand(InstrumentId), ICommand<Models.Transaction>
{
    public static ValueTask<Create> BindAsync(HttpContext httpContext) => BindHelper.BindWithInstrumentIdAsync<Create>(httpContext);
}

internal class CreateHandler(IInstrumentRepository accountRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork) :  ICommandHandler<Create, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(Create command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var instrumentId, out var amount, out var description, out var reference, out var transactionTime);

        var account = await accountRepository.Get(instrumentId, cancellationToken);
        if (account is not Domain.Entities.Instrument.TransactionInstrument transactionAccount)
        {
            throw new InvalidOperationException("Not a transaction account.");
        }

        TransactionType transactionType = amount < 0 ? TransactionType.Debit : TransactionType.Credit;

        Transaction transaction = new()
        {
            Account = transactionAccount,
            Amount = amount,
            Description = description,
            Reference = reference,
            Source = "Web",
            TransactionTime = transactionTime.LocalDateTime,
            TransactionType = transactionType,
        };

        transactionRepository.Add(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
