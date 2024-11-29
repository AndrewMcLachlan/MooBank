using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

internal record RemoveTag(Guid InstrumentId, int RuleId, int TagId) : ICommand;

internal class RemoveTagHandler(IInstrumentRepository instrumentRepository, IUnitOfWork unitOfWork) : ICommandHandler<RemoveTag>
{
    public async ValueTask Handle(RemoveTag request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(request.InstrumentId, cancellationToken);

        var entity = instrument.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException($"Rule with id {request.RuleId} was not found");

        var tag = entity.Tags.SingleOrDefault(t => t.Id == request.TagId) ?? throw new NotFoundException($"Tag with id {request.TagId} was not found");

        entity.Tags.Remove(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
