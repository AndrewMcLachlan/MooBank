using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tags.Queries;

internal record Get(int Id) : IQuery<Tag>;

internal class GetHandler(IQueryable<Domain.Entities.Tag.Tag> tags) : IQueryHandler<Get, Tag>
{
    public async ValueTask<Tag> Handle(Get request, CancellationToken cancellationToken) =>
        await tags.Include(t => t.Settings).Where(t => t.Id == request.Id).ToModel().SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
}
