using Asm.MooBank.Models;
using Microsoft.EntityFrameworkCore.Query;
using TransactionTagEntity = Asm.MooBank.Domain.Entities.TransactionTags.TransactionTag;

namespace Asm.MooBank.Queries.TransactionTags;

public record GetTransactionTagsHierarchy : IQuery<TransactionTagHierarchy>;

internal class GetTransactionTagsHierarchyHandler : IQueryHandler<GetTransactionTagsHierarchy, TransactionTagHierarchy>
{
    private readonly IQueryable<TransactionTagEntity> _tags;


    public GetTransactionTagsHierarchyHandler(IQueryable<TransactionTagEntity> tags)
    {
        _tags = tags;
    }

    public async Task<TransactionTagHierarchy> Handle(GetTransactionTagsHierarchy request, CancellationToken cancellationToken)
    {
        const int maxLevels = 5;

        IIncludableQueryable<TransactionTagEntity, IEnumerable<TransactionTagEntity>> query = _tags.Where(t => !t.Deleted && !t.TaggedTo.Any()).Include(t => t.Tags.Where(t => !t.Deleted));

        for (int i = 0; i < maxLevels; i++)
        {
            query = query.ThenInclude(t => t.Tags.Where(t => !t.Deleted));
        }


        var tags = await query.ToListAsync(cancellationToken).ToHierarchyModelAsync(cancellationToken);

        var tagLevel = tags;

        Dictionary<int, int> levels = new();

        for (int i = 1; i <= maxLevels; i++)
        {
            levels.Add(i, tagLevel.SelectMany(t => t.Tags).Count());
            tagLevel = tagLevel.SelectMany(t => t.Tags);
        }

        return new()
        {
            Levels = levels,
            Tags = tags,
        };
    }
}
