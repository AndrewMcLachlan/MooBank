using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Models;
using Asm.MooBank.Queries;
using Microsoft.EntityFrameworkCore.Query;
using TransactionTagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetTagsHierarchy : IQuery<TagHierarchy>;

internal class GetTagsHierarchyHandler : QueryHandlerBase, IQueryHandler<GetTagsHierarchy, TagHierarchy>
{
    private readonly IQueryable<TransactionTagEntity> _tags;


    public GetTagsHierarchyHandler(IQueryable<TransactionTagEntity> tags, AccountHolder accountHolder) : base(accountHolder)
    {
        _tags = tags;
    }

    public async ValueTask<TagHierarchy> Handle(GetTagsHierarchy request, CancellationToken cancellationToken)
    {
        const int maxLevels = 5;

        IIncludableQueryable<TransactionTagEntity, IEnumerable<TransactionTagEntity>> query = _tags.Where(t => t.FamilyId == AccountHolder.FamilyId && !t.Deleted && !t.TaggedTo.Any()).Include(t => t.Tags.Where(t => !t.Deleted));

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
