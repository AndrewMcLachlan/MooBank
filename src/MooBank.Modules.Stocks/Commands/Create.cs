using System.ComponentModel;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Stocks.Commands;

[DisplayName("CreateStock")]
public sealed record Create() : ICommand<Models.StockHolding>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public required int Quantity { get; init; }
    public required decimal Fees { get; init; }
    public required bool ShareWithFamily { get; init; }
    public Guid? GroupId { get; init; }
}

internal class CreateHandler(IStockHoldingRepository repository, IUnitOfWork unitOfWork, User user, ISecurity security, ICurrencyConverter currencyConverter) : ICommandHandler<Create, Models.StockHolding>
{
    public async ValueTask<Models.StockHolding> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            await security.AssertGroupPermission(command.GroupId.Value);
        }

        var entity = Domain.Entities.StockHolding.StockHolding.Create(command.Name, command.Description, command.Symbol, command.ShareWithFamily, command.Price);

        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        repository.Add(entity);

        // TODO: Move to domain event
        entity.Transactions.Add(new()
        {
            Quantity = command.Quantity,
            Fees = command.Fees,
            Price = command.Price,
            TransactionDate = DateTime.UtcNow,
            TransactionType = TransactionType.Credit,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await entity.ToModel(currencyConverter, cancellationToken);
    }
}
