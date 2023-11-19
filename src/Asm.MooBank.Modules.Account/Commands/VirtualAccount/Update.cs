using System.Text.Json.Serialization;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
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

internal class UpdateHandler(IAccountRepository accountRepository, ITransactionRepository transactionRepository, AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Account.VirtualAccount>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Account.VirtualAccount> Handle(Update request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.GetVirtualAccount(request.AccountId, request.VirtualAccountId, cancellationToken);

        account.Name = request.Name;
        account.Description = request.Description;

        var amount = account.Balance - request.CurrentBalance;

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

        account.Balance = request.CurrentBalance;


        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return account;
    }
}
