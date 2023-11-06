using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Modules.Tag.Models;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record Update(int TagId, UpdateTag Tag) : ICommand<MooBank.Models.Tag>;

internal sealed class UpdateHandler(ITagRepository transactionTagRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Update, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await transactionTagRepository.Get(request.TagId, cancellationToken);

        await Security.AssertFamilyPermission(tag.FamilyId);

        tag.Name = request.Tag.Name;
        tag.Settings.ExcludeFromReporting = request.Tag.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.Tag.ApplySmoothing;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
