using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Rules;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Modules.Accounts.Commands.Rule;

public record AddTag(Guid AccountId, int RuleId, int TagId) : ICommand<Models.Rules.Rule>;

internal class AddTagHandler(IInstrumentRepository accountRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<AddTag, Models.Rules.Rule>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;
    private readonly ITagRepository _tagsRepository = tagRepository;

    public async ValueTask<Models.Rules.Rule> Handle(AddTag request, CancellationToken cancellationToken)
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
