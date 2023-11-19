using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

internal record RemoveTag(Guid AccountId, int RuleId, int TagId) : ICommand;

internal class RemoveTagHandler(IAccountRepository accountRepository, MooBank.Models.AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<RemoveTag>
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async ValueTask Handle(RemoveTag request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        var entity = account.Rules.SingleOrDefault(r => r.Id == request.RuleId) ?? throw new NotFoundException($"Rule with id {request.RuleId} was not found");

        var tag = entity.Tags.SingleOrDefault(t => t.Id == request.TagId) ?? throw new NotFoundException($"Tag with id {request.TagId} was not found");

        entity.Tags.Remove(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
