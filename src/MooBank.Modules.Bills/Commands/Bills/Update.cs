using System.ComponentModel;
using Asm.Domain;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Commands.Bills;

[DisplayName("UpdateBill")]
public record Update(Guid InstrumentId, int Id, UpdateBill Bill) : ICommand<Bill>;

internal class UpdateHandler(IUnitOfWork unitOfWork, Domain.Entities.Utility.IAccountRepository accountRepository) : ICommandHandler<Update, Bill>
{
    public async ValueTask<Bill> Handle(Update command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetWithBill(command.InstrumentId, command.Id, cancellationToken) ?? throw new NotFoundException("Account not found");

        var bill = account.Bills.SingleOrDefault(b => b.Id == command.Id) ?? throw new NotFoundException("Bill not found");

        bill.Update(
            command.Bill.InvoiceNumber,
            command.Bill.IssueDate,
            command.Bill.CurrentReading,
            command.Bill.PreviousReading,
            command.Bill.CostsIncludeGST,
            command.Bill.Periods.Select(p => new Domain.Entities.Utility.Period
            {
                PeriodStart = p.PeriodStart,
                PeriodEnd = p.PeriodEnd,
                ServiceCharges = p.ServiceCharges.Select(sc => new Domain.Entities.Utility.ServiceCharge
                {
                    ChargeTypeId = sc.ChargeTypeId,
                    ChargePerDay = sc.ChargePerDay,
                }).ToList(),
                Usages = p.Usages.Select(u => new Domain.Entities.Utility.Usage
                {
                    UsageType = u.UsageType,
                    PricePerUnit = u.PricePerUnit,
                    TotalUsage = u.TotalUsage,
                }).ToList(),
            }),
            command.Bill.Discounts.Select(d => new Domain.Entities.Utility.Discount
            {
                DiscountAmount = d.DiscountAmount,
                DiscountPercent = d.DiscountPercent,
                Reason = d.Reason,
            }));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return bill.ToModel();
    }
}
