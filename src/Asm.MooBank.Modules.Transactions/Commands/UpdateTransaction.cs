using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateTransaction(Guid AccountId, Guid Id, string? Notes, IEnumerable<Models.TransactionSplit> Splits, bool ExcludeFromReporting = false) : ICommand<Models.Transaction>
{
    public static async ValueTask<UpdateTransaction?> BindAsync(HttpContext httpContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>();

        if (!Guid.TryParse(httpContext.Request.RouteValues["accountId"] as string, out Guid accountId)) throw new BadHttpRequestException("invalid account ID");
        if (!Guid.TryParse(httpContext.Request.RouteValues["id"] as string, out Guid id)) throw new BadHttpRequestException("invalid transaction ID");

        var update = await System.Text.Json.JsonSerializer.DeserializeAsync<UpdateTransaction>(httpContext.Request.Body, options.Value.SerializerOptions, cancellationToken: httpContext.RequestAborted);
        return update! with { AccountId = accountId, Id = id };
    }
}

internal class UpdateTransactionHandler(ITransactionRepository transactionRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<UpdateTransaction, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(UpdateTransaction request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await transactionRepository.Get(request.Id, cancellationToken);

        #region Splits
        var splitsToRemove = entity.Splits.Where(o => !request.Splits.Any(ro => ro.Id == o.Id)).ToList();
        var splitsToAdd = request.Splits.Where(o => !entity.Splits.Any(ro => ro.Id == o.Id)).ToList();
        var splitsToUpdate = request.Splits.Where(o => entity.Splits.Any(ro => ro.Id == o.Id)).ToList();

        foreach (var split in splitsToRemove)
        {
            if (entity.Splits.Count > 1)
            {
                entity.RemoveSplit(split);
            }
            else
            {
                split.UpdateTags([]);
                split.OffsetBy.Clear();
            }
        }

        foreach (var split in splitsToAdd)
        {
            entity.Splits.Add(new TransactionSplit(split.Id)
            {
                Amount = split.Amount,
                TransactionId = entity.TransactionId,
                Tags = (await tagRepository.Get(split.Tags.Select(t => t.Id), cancellationToken)).ToList(),
            });
        }

        foreach (var split in splitsToUpdate)
        {
            var splitEntity = entity.Splits.Single(s => s.Id == split.Id);
            splitEntity.Amount = split.Amount;
            var tags = await tagRepository.Get(split.Tags.Select(t => t.Id), cancellationToken);
            splitEntity.UpdateTags(tags);

            #region Offsets
            var offsetsToRemove = splitEntity.OffsetBy.Where(o => !split.OffsetBy.Any(ro => ro.Transaction.Id == o.OffsetTransactionId)).ToList();
            var offsetsToAdd = split.OffsetBy.Where(o => !splitEntity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.Transaction.Id)).ToList();
            var offsetsToUpdate = split.OffsetBy.Where(o => splitEntity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.Transaction.Id)).ToList();

            foreach (var offset in offsetsToRemove)
            {
                splitEntity.RemoveOffset(offset);
            }

            foreach (var offset in offsetsToAdd)
            {
                splitEntity.OffsetBy.Add(new TransactionOffset
                {
                    Amount = offset.Amount,
                    TransactionSplitId = splitEntity.Id,
                    OffsetTransactionId = offset.Transaction.Id,
                });
            }

            foreach (var offset in offsetsToUpdate)
            {
                var offsetEntity = splitEntity.OffsetBy.First(o => o.OffsetTransactionId == offset.Transaction.Id);
                offsetEntity.Amount = offset.Amount;
            }
            #endregion
        }
        #endregion

        entity.Notes = request.Notes;
        entity.ExcludeFromReporting = request.ExcludeFromReporting;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
