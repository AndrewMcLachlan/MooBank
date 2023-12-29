using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models.Account;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.VirtualAccount;

public record Update(Guid AccountId, Guid VirtualAccountId, string Name, string Description, decimal CurrentBalance) : ICommand<Models.Account.VirtualAccount>
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

internal class UpdateHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, ISecurity security, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Update, Models.Account.VirtualAccount>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Update command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);

        var parentAccount = await _accountRepository.Get(command.AccountId, new VirtualAccountSpecification(), cancellationToken);

        var account = parentAccount.VirtualAccounts.SingleOrDefault(a => a.Id == command.VirtualAccountId) ?? throw new NotFoundException();

        account.Name = command.Name;
        account.Description = command.Description;

        var amount = account.Balance - command.CurrentBalance;

        //TODO: Should be done via domain event
        _transactionRepository.Add(new Domain.Entities.Transactions.Transaction
        {
            Account = account,
            Amount = amount,
            Description = "Balance adjustment",
            Source = "Web",
            TransactionTime = DateTime.Now,
            TransactionType = amount > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
        });

        account.Balance = command.CurrentBalance;


        await unitOfWork.SaveChangesAsync(cancellationToken);
        return account.ToModel(currencyConverter);
    }
}
