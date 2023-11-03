using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Tag.Queries;

public record GetAll() : IQuery<IEnumerable<Models.Tag>>;

internal class GetAllHandler : QueryHandlerBase, IQueryHandler<GetAll, IEnumerable<Models.Tag>>
{
    private readonly IQueryable<Domain.Entities.Tag.Tag> _tags;

    public GetAllHandler(IQueryable<Domain.Entities.Tag.Tag> tags, AccountHolder accountHolder, ISecurity security) : base(accountHolder)
    {
        _tags = tags;
    }

    public async ValueTask<IEnumerable<Models.Tag>> Handle(GetAll _, CancellationToken cancellationToken) =>
        (await _tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .Where(t => t.FamilyId == AccountHolder.FamilyId && !t.Deleted).ToListAsync(cancellationToken).ToModelAsync(cancellationToken)).OrderBy(t => t.Name);
}
