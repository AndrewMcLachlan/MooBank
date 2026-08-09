using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Group;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Groups.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Groups.Commands;

/// <summary>
/// Puts the caller's groups in the order given.
/// </summary>
/// <remarks>
/// Takes the whole list rather than "move this one to position n" because the whole list is what
/// the page already has, and sending it entire makes the command idempotent: the same request
/// applied twice leaves the same order, whatever happened in between.
/// </remarks>
[DisplayName("ReorderGroups")]
public sealed record Reorder(GroupOrder Order) : ICommand<IEnumerable<Models.Group>>;

internal class ReorderHandler(IGroupRepository groupRepository, IQueryable<Domain.Entities.Group.Group> groups, IUnitOfWork unitOfWork, ISecurity security, User user) : ICommandHandler<Reorder, IEnumerable<Models.Group>>
{
    public async ValueTask<IEnumerable<Models.Group>> Handle(Reorder request, CancellationToken cancellationToken)
    {
        var requested = request.Order.GroupIds.ToList();

        if (requested.Distinct().Count() != requested.Count)
        {
            throw new BadHttpRequestException("The same group appears more than once");
        }

        var owned = await groups.Where(g => g.OwnerId == user.Id)
                                .Select(g => g.Id)
                                .ToListAsync(cancellationToken);

        // The order has to name every group the caller has, or the ones left out keep a position
        // that now means something different and the list comes back interleaved. A client working
        // from a stale copy is exactly when that happens, so it is refused rather than half-applied.
        if (requested.Count != owned.Count || !owned.All(requested.Contains))
        {
            throw new BadHttpRequestException("The order must list every one of your groups, exactly once");
        }

        for (var position = 0; position < requested.Count; position++)
        {
            var entity = await groupRepository.Get(requested[position], cancellationToken);

            // Belt and braces: the ids were matched against the caller's own groups above, so this
            // cannot fail. It audits the denial if the two ever disagree.
            await security.AssertGroupPermission(entity);

            entity.SortOrder = position;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await groups.Where(g => g.OwnerId == user.Id)
                           .OrderBy(g => g.SortOrder).ThenBy(g => g.Name)
                           .ToModel().ToListAsync(cancellationToken);
    }
}
