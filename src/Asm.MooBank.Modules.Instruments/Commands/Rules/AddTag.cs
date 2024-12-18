using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Rules;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.Rules;

public record AddTag(Guid InstrumentId, int RuleId, int TagId) : ICommand<Rule>;

internal class AddTagHandler(IInstrumentRepository instrumentRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork) : ICommandHandler<AddTag, Rule>
{
    public async ValueTask<Rule> Handle(AddTag request, CancellationToken cancellationToken)
    {
        var instrument = await instrumentRepository.Get(request.InstrumentId, cancellationToken);

        var rule = instrument.Rules.Where(r => r.Id == request.RuleId).SingleOrDefault() ?? throw new NotFoundException($"Rule with ID {request.RuleId} not found");

        if (rule.Tags.Any(t => t.Id == request.TagId)) return rule.ToModel();

        rule.Tags.Add(await tagRepository.Get(request.TagId, cancellationToken));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
