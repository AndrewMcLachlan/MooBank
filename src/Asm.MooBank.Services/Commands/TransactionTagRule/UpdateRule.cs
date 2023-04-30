using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Models.Commands.TransactionTagRules;

namespace Asm.MooBank.Services.Commands.TransactionTagRule;

internal class UpdateRuleHandler : ICommandHandler<UpdateRule, Models.TransactionTagRule>
{
    private readonly ITransactionTagRuleRepository _transactionTagRuleRepository;
    private readonly ISecurityRepository _security;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRuleHandler(ITransactionTagRuleRepository transactionTagRuleRepository, ISecurityRepository securityRepository, IUnitOfWork unitOfWork)
    {
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _security = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.TransactionTagRule> Handle(UpdateRule request, CancellationToken cancellationToken)
    {
        var entity = await _transactionTagRuleRepository.Get(request.Rule.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
