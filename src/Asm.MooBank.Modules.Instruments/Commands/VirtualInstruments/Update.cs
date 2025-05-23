﻿using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record Update(Guid InstrumentId, Guid VirtualAccountId, string Name, string Description, decimal CurrentBalance) : ICommand<VirtualInstrument>
{
    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["virtualAccountId"] as string, out Guid virtualAccountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { InstrumentId = accountId, VirtualAccountId = virtualAccountId };
    }
}

internal class UpdateHandler(IInstrumentRepository instrumentRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Update, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(Update command, CancellationToken cancellationToken)
    {
        var parentInstrument = await instrumentRepository.Get(command.InstrumentId, new VirtualAccountSpecification(), cancellationToken);

        var instrument = parentInstrument.VirtualInstruments.SingleOrDefault(a => a.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        instrument.Name = command.Name;
        instrument.Description = command.Description;

        var amount = instrument.Balance - command.CurrentBalance;

        //TODO: Should be done via domain event
        transactionRepository.Add(new Transaction
        {
            Account = instrument,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.Credit : TransactionType.Debit,
            TransactionSubType = TransactionSubType.BalanceAdjustment,
        });

        instrument.Balance = command.CurrentBalance;


        await unitOfWork.SaveChangesAsync(cancellationToken);
        return instrument.ToModel(currencyConverter);
    }
}
