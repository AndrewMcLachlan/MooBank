using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Stocks.Commands;
public sealed record Update : ICommand<Models.StockHolding>
{
    public Guid AccountId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool ShareWithFamily { get; init; }
    public required decimal CurrentPrice { get; init; }
    public Guid? AccountGroupId { get; init; }

    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["id"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { AccountId = accountId };
    }
}

internal class UpdateHandler(IStockHoldingRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security) :  ICommandHandler<Update, Models.StockHolding>
{
    public async ValueTask<Models.StockHolding> Handle(Update command, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(command.AccountId);
        if (command.AccountGroupId != null)
        {
            security.AssertAccountGroupPermission(command.AccountGroupId.Value);
        }

        var stockHolding = await repository.Get(command.AccountId, new IncludeSpecification(), cancellationToken) ?? throw new NotFoundException();

        stockHolding.Name = command.Name;
        stockHolding.Description = command.Description;
        stockHolding.ShareWithFamily = command.ShareWithFamily;
        stockHolding.CurrentPrice = command.CurrentPrice;

        stockHolding.SetAccountGroup(command.AccountGroupId, user.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return stockHolding.ToModel();
    }
}
