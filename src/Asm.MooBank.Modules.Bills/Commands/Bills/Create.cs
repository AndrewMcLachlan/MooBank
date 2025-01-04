using Asm.Domain;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Commands.Bills;

public record Create(Guid InstrumentId, CreateBill Bill) : ICommand<Bill>;


internal class CreateHandler(IUnitOfWork unitOfWork, Domain.Entities.Utility.IAccountRepository accountRepository) : ICommandHandler<Create, Bill>
{
    public async ValueTask<Bill> Handle(Create command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.Get(command.InstrumentId, cancellationToken) ?? throw new NotFoundException("Account not found");

        var bill = new Domain.Entities.Utility.Bill
        {
            AccountId = account.Id,
            Cost = command.Bill.Cost,
            CostsIncludeGST = command.Bill.CostsIncludeGST,
            CurrentReading = command.Bill.CurrentReading,
            Discounts = command.Bill.Discounts.Select(d => new Domain.Entities.Utility.Discount
            {
                DiscountAmount = d.DiscountAmount,
                DiscountPercent = d.DiscountPercent,
                Reason = d.Reason,
            }).ToList(),
            IssueDate = command.Bill.IssueDate,
            InvoiceNumber = command.Bill.InvoiceNumber,
            Periods = command.Bill.Periods.Select(p => new Domain.Entities.Utility.Period
            {
                PeriodEnd = p.PeriodEnd,
                PeriodStart = p.PeriodStart,
                ServiceCharge = new()
                {
                    ChargePerDay = p.ChargePerDay,
                },
                Usage = new()
                {
                    PricePerUnit = p.PricePerUnit,
                    TotalUsage = p.TotalUsage,
                }
            }).ToList(),
            PreviousReading = command.Bill.PreviousReading,
            Total = command.Bill.Total,
        };

        account.Bills.Add(bill);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return bill.ToModel();

    }
}
