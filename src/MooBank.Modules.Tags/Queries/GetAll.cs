using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetAll() : IQuery<IEnumerable<Tag>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Tag.Tag> tags) : IQueryHandler<GetAll, IEnumerable<Tag>>
{
    public async ValueTask<IEnumerable<Tag>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
        .OrderBy(t => t.Name).ToModel().ToListAsync(cancellationToken);
}
