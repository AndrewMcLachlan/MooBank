using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Models;
using Microsoft.EntityFrameworkCore;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetTagsGraph : IQuery<TagGraph>;

internal sealed class GetTagsGraphHandler(
    IQueryable<TagEntity> tags) : IQueryHandler<GetTagsGraph, TagGraph>
{
    public async ValueTask<TagGraph> Handle(GetTagsGraph request, CancellationToken cancellationToken)
    {
        var rows = await tags
            .Include(t => t.Settings)
            .Include(t => t.Tags)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Colour,
                t.Settings.ApplySmoothing,
                t.Settings.ExcludeFromReporting,
                ChildIds = t.Tags
                    .Select(c => c.Id)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var nodes = rows
            .Select(r => new TagNode
            {
                Id = r.Id,
                Name = r.Name,
                Colour = r.Colour,
                Settings = new TagNodeSettings
                {
                    ApplySmoothing = r.ApplySmoothing,
                    ExcludeFromReporting = r.ExcludeFromReporting,
                },
            })
            .ToList();

        var edges = rows
            .SelectMany(r => r.ChildIds.Select(cid => new TagEdge { ParentId = r.Id, ChildId = cid }))
            .ToList();

        return new TagGraph
        {
            Nodes = nodes,
            Edges = edges,
        };
    }
}
