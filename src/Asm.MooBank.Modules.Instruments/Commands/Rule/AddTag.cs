using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Queries.Rules;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.Rule;

public record AddTag(Guid AccountId, int RuleId, int TagId) : ICommand<Queries.Rules.Rule>;

internal class AddTagHandler(IInstrumentRepository accountRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<AddTag, Queries.Rules.Rule>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;
    private readonly ITagRepository _tagsRepository = tagRepository;

    public async ValueTask<Queries.Rules.Rule> Handle(AddTag request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        var rule = account.Rules.Where(r => r.Id == request.RuleId).SingleOrDefault() ?? throw new NotFoundException($"Rule with ID {request.RuleId} not found");

        if (rule.Tags.Any(t => t.Id == request.TagId)) return rule.ToModel();

        rule.Tags.Add(await _tagsRepository.Get(request.TagId, cancellationToken));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
