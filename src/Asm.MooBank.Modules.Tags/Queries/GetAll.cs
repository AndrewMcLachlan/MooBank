﻿using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetAll() : IQuery<IEnumerable<Tag>>;

internal class GetAllHandler(IQueryable<Domain.Entities.Tag.Tag> tags, User user) : IQueryHandler<GetAll, IEnumerable<Tag>>
{
    private readonly IQueryable<Domain.Entities.Tag.Tag> _tags = tags;

    public async ValueTask<IEnumerable<Tag>> Handle(GetAll _, CancellationToken cancellationToken) =>
        await _tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .ThenInclude(t => t.Tags)
        .Where(t => t.FamilyId == user.FamilyId && !t.Deleted).OrderBy(t => t.Name).ToModel().ToListAsync(cancellationToken);
}
