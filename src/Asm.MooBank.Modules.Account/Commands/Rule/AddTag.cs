using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Account.Models;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record AddTag(Guid AccountId, int RuleId, int TagId) : ICommand<Models.Rule>;

internal class AddTagHandler(IInstrumentRepository accountRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<AddTag, Models.Rule>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;
    private readonly ITagRepository _tagsRepository = tagRepository;

    public async ValueTask<Models.Rule> Handle(AddTag request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        var rule = account.Rules.Where(r => r.Id == request.RuleId).SingleOrDefault() ?? throw new NotFoundException($"Rule with ID {request.RuleId} not found");

        if (rule.Tags.Any(t => t.Id == request.TagId)) return rule.ToModel();

        rule.Tags.Add(await _tagsRepository.Get(request.TagId, cancellationToken));

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToModel();
    }
}
