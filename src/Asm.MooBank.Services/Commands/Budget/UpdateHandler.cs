using Asm.Domain;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models.Commands.Budget;

namespace Asm.MooBank.Services.Commands.Budget
{
    internal class UpdateHandler : CommandHandlerBase, ICommandHandler<Update, Models.BudgetLine>
    {
        private readonly IBudgetRepository _budgetRepository;

        public UpdateHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurityRepository security) : base(unitOfWork, security)
        {
            _budgetRepository = budgetRepository;
        }

        public async Task<Models.BudgetLine> Handle(Update request, CancellationToken cancellationToken)
        {
            Security.AssertAccountPermission(request.AccountId);

            var entity = await _budgetRepository.Get(request.BudgetLine.Id, cancellationToken);

            entity.Amount = request.BudgetLine.Amount;
            entity.TagId = request.BudgetLine.TagId;

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }
    }
}
