using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetAll() : IQuery<IEnumerable<Tag>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Tag.Tag> tags, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<GetAll, IEnumerable<Tag>>
{
    private readonly IQueryable<Domain.Entities.Tag.Tag> _tags = tags;

    public async ValueTask<IEnumerable<Tag>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await _tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .Where(t => t.FamilyId == AccountHolder.FamilyId && !t.Deleted).OrderBy(t => t.Name).ToModel().ToListAsync(cancellationToken);
}
