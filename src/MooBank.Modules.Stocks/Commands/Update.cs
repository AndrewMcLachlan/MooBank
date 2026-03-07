using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Stocks.Commands;

[DisplayName("UpdateStock")]
public sealed record Update : ICommand<Models.StockHolding>
{
    public Guid InstrumentId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool ShareWithFamily { get; init; }
    public required decimal CurrentPrice { get; init; }
    public Guid? GroupId { get; init; }

    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["instrumentId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { InstrumentId = accountId };
    }
}

internal class UpdateHandler(IStockHoldingRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security, ICurrencyConverter currencyConverter) : ICommandHandler<Update, Models.StockHolding>
{
    public async ValueTask<Models.StockHolding> Handle(Update command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            security.AssertGroupPermission(command.GroupId.Value);
        }

        var stockHolding = await repository.Get(command.InstrumentId, new IncludeSpecification(), cancellationToken) ?? throw new NotFoundException();

        stockHolding.Name = command.Name;
        stockHolding.Description = command.Description;
        stockHolding.ShareWithFamily = command.ShareWithFamily;
        stockHolding.CurrentPrice = command.CurrentPrice;

        stockHolding.SetGroup(command.GroupId, user.Id);

        repository.Update(stockHolding);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return stockHolding.ToModel(currencyConverter);
    }
}
