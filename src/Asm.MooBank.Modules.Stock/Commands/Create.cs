using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;

namespace Asm.MooBank.Modules.Stocks.Commands;
public sealed record Create() : ICommand<Models.StockHolding>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public required int Quantity { get; init; }
    public required decimal Fees { get; init; }
    public required bool ShareWithFamily { get; init; }
    public Guid? AccountGroupId { get; init; }
}

internal class CreateHandler(IStockHoldingRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security) :  ICommandHandler<Create, Models.StockHolding>
{
    public async ValueTask<Models.StockHolding> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.AccountGroupId != null)
        {
            security.AssertAccountGroupPermission(command.AccountGroupId.Value);
        }

        Domain.Entities.StockHolding.StockHolding entity = new(Guid.Empty)
        {
            Name = command.Name,
            Description = command.Description,
            Symbol = command.Symbol,
            ShareWithFamily = command.ShareWithFamily,
            CurrentPrice = command.Price,
        };

        entity.SetAccountHolder(user.Id);
        entity.SetAccountGroup(command.AccountGroupId, user.Id);

        repository.Add(entity);

        entity.Transactions.Add(new()
        {
            Quantity = command.Quantity,
            Fees = command.Fees,
            Price = command.Price,
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.Credit,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
