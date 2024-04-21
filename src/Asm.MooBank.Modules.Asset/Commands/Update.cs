using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Asset.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Asset.Commands;
public sealed record Update : ICommand<Models.Asset>
{
    public Guid AccountId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool ShareWithFamily { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal CurrentBalance{ get; init; }
    public Guid? AccountGroupId { get; init; }

    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["id"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { AccountId = accountId };
    }
}

internal class UpdateHandler(IAssetRepository repository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Update command, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(command.AccountId);
        if (command.AccountGroupId != null)
        {
            Security.AssertAccountGroupPermission(command.AccountGroupId.Value);
        }

        var Asset = await repository.Get(command.AccountId, new IncludeSpecification(), cancellationToken) ?? throw new NotFoundException();

        Asset.Balance = command.CurrentBalance;
        Asset.Name = command.Name;
        Asset.Description = command.Description;
        Asset.ShareWithFamily = command.ShareWithFamily;
        Asset.PurchasePrice = command.PurchasePrice;

        Asset.SetAccountGroup(command.AccountGroupId, AccountHolder.Id);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return Asset.ToModel();
    }
}
