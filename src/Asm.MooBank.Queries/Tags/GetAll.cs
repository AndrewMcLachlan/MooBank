using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Tags;

public record GetAll() : IQuery<IEnumerable<Tag>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<Tag>>
{
    private readonly IQueryable<Domain.Entities.Tag.Tag> _tags;

    public GetAllHandler(IQueryable<Domain.Entities.Tag.Tag> tags, AccountHolder accountHolder, ISecurity security) : base(accountHolder)
    {
        _tags = tags;
    }

    public async Task<IEnumerable<Tag>> Handle(GetAll request, CancellationToken cancellationToken) =>
        (await _tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .Where(t => t.FamilyId == AccountHolder.FamilyId && !t.Deleted).ToListAsync(cancellationToken).ToModelAsync(cancellationToken)).OrderBy(t => t.Name);

}
