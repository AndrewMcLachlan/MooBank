using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Asset;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Assets.Models;
using Asm.MooBank.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Assets.Commands;

[DisplayName("UpdateAsset")]
public sealed record Update : ICommand<Models.Asset>
{
    public Guid AccountId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required bool ShareWithFamily { get; init; }
    public decimal? PurchasePrice { get; init; }
    public decimal CurrentBalance { get; init; }
    public Guid? GroupId { get; init; }

    public static async ValueTask<Update?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["id"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { AccountId = accountId };
    }
}

internal class UpdateHandler(IAssetRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security, ICurrencyConverter currencyConverter) :  ICommandHandler<Update, Models.Asset>
{
    public async ValueTask<Models.Asset> Handle(Update command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            security.AssertGroupPermission(command.GroupId.Value);
        }

        var entity = await repository.Get(command.AccountId, new IncludeSpecification(), cancellationToken) ?? throw new NotFoundException();

        entity.Value = command.CurrentBalance;
        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.ShareWithFamily = command.ShareWithFamily;
        entity.PurchasePrice = command.PurchasePrice;

        entity.SetGroup(command.GroupId, user.Id);

        repository.Update(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
