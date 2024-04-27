using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Modules.Accounts.Commands.VirtualAccount;

public record Update(Guid AccountId, Guid VirtualAccountId, string Name, string Description, decimal CurrentBalance) : ICommand<VirtualInstrument>
{
    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["virtualAccountId"] as string, out Guid virtualAccountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { AccountId = accountId, VirtualAccountId = virtualAccountId };
    }
}

internal class UpdateHandler(IInstrumentRepository instrumentRepository, ITransactionRepository transactionRepository, ISecurity security, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Update, VirtualInstrument>
{
    private readonly IInstrumentRepository _accountRepository = instrumentRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<VirtualInstrument> Handle(Update command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);

        var parentInstrument = await _accountRepository.Get(command.AccountId, new VirtualAccountSpecification(), cancellationToken);

        var instrument = parentInstrument.VirtualInstruments.SingleOrDefault(a => a.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        instrument.Name = command.Name;
        instrument.Description = command.Description;

        var amount = instrument.Balance - command.CurrentBalance;

        //TODO: Should be done via domain event
        _transactionRepository.Add(new Transaction
        {
            Account = instrument,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        instrument.Balance = command.CurrentBalance;


        await unitOfWork.SaveChangesAsync(cancellationToken);
        return instrument.ToModel(currencyConverter);
    }
}
