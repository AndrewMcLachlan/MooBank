using System.ComponentModel;
using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.StockHolding;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Stocks.Models;
using Microsoft.AspNetCore.Http;

namespace Asm.MooBank.Modules.Stocks.Commands.Transactions;

[DisplayName("CreateStockTransaction")]
public sealed record Create(Guid InstrumentId, int Quantity, decimal Price, decimal Fees, string Description, DateTime Date) : InstrumentIdCommand(InstrumentId), ICommand<StockTransaction>
{
    public static ValueTask<Create> BindAsync(HttpContext httpContext) => BindHelper.BindWithInstrumentIdAsync<Create>("instrumentId", httpContext);
}

internal class CreateHandler(IStockHoldingRepository stockHoldingRepository, IUnitOfWork unitOfWork) : ICommandHandler<Create, StockTransaction>
{
    public async ValueTask<StockTransaction> Handle(Create command, CancellationToken cancellationToken)
    {
        var stockHolding = await stockHoldingRepository.Get(command.InstrumentId, cancellationToken);

        var transaction = new Domain.Entities.Transactions.StockTransaction(Guid.Empty)
        {
            AccountId = command.InstrumentId,
            Quantity = command.Quantity,
            Price = command.Price,
            Fees = command.Fees,
            Description = command.Description,
            TransactionDate = command.Date,
            TransactionType = command.Quantity > 0 ? TransactionType.Credit : TransactionType.Debit,
        };

        stockHolding.Transactions.Add(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return transaction.ToModel();
    }
}
