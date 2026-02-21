using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Tags.Commands;

[DisplayName("UpdateTag")]
public sealed record Update([FromRoute] int Id, [FromBody] UpdateTag Tag) : ICommand<MooBank.Models.Tag>;

internal sealed class UpdateHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Update, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.Get(request.Id, cancellationToken);

        await security.AssertFamilyPermission(tag.FamilyId);

        tag.Name = request.Tag.Name;
        tag.Colour = request.Tag.Colour;
        tag.Settings.ExcludeFromReporting = request.Tag.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.Tag.ApplySmoothing;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
