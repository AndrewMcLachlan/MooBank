using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Tags.Models;

namespace Asm.MooBank.Modules.Tags.Commands;

public sealed record Update(int TagId, UpdateTag Tag) : ICommand<MooBank.Models.Tag>;

internal sealed class UpdateHandler(ITagRepository transactionTagRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await transactionTagRepository.Get(request.TagId, cancellationToken);

        await Security.AssertFamilyPermission(tag.FamilyId);

        tag.Name = request.Tag.Name;
        tag.Settings.ExcludeFromReporting = request.Tag.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.Tag.ApplySmoothing;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
