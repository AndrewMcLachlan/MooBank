using Asm.MooBank.Models;
using Asm.MooBank.Queries;

namespace Asm.MooBank.Modules.Tag.Queries;

internal record Get(int Id) : IQuery<MooBank.Models.Tag>;

internal class GetHandler(IQueryable<Domain.Entities.Tag.Tag> tags, AccountHolder accountHolder) : QueryHandlerBase(accountHolder), IQueryHandler<Get, MooBank.Models.Tag>
{
    private readonly IQueryable<Domain.Entities.Tag.Tag> _tags = tags;

    public async ValueTask<MooBank.Models.Tag> Handle(Get request, CancellationToken cancellationToken) =>
        (await _tags.Where(t => t.Id == request.Id && t.FamilyId == AccountHolder.FamilyId).Select(t => t.ToModel()).SingleOrDefaultAsync(cancellationToken)) ?? throw new NotFoundException();
}
