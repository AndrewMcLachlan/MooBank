using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Modules.Transactions.Models.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateTransaction(Guid InstrumentId, Guid Id, string? Notes, IEnumerable<MooBank.Models.TransactionSplit> Splits, bool ExcludeFromReporting = false) : ICommand<MooBank.Models.Transaction>
{
    public static async ValueTask<UpdateTransaction?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["instrumentId"] as string, out Guid instrumentId)) throw new BadHttpRequestException("invalid account ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["id"] as string, out Guid id)) throw new BadHttpRequestException("invalid transaction ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<UpdateTransaction>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { InstrumentId = instrumentId, Id = id };
    }
}

internal class UpdateTransactionHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork) : ICommandHandler<UpdateTransaction, MooBank.Models.Transaction>
{
    public async ValueTask<MooBank.Models.Transaction> Handle(UpdateTransaction request, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.Get(request.Id, new IncludeSplitsSpecification(), cancellationToken);

        transaction.UpdateSplits(request.Splits);

        transaction.Notes = request.Notes;
        transaction.ExcludeFromReporting = request.ExcludeFromReporting;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
