using Asm.Domain;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Commands.Bills;

public record Import(IEnumerable<ImportBill> Bills) : ICommand<ImportResult>;

internal class ImportHandler(
    IUnitOfWork unitOfWork,
    Domain.Entities.Utility.IAccountRepository accountRepository) : ICommandHandler<Import, ImportResult>
{
    public async ValueTask<ImportResult> Handle(Import command, CancellationToken cancellationToken)
    {
        var userAccounts = (await accountRepository.Get(cancellationToken)).ToList();

        var errors = new List<string>();
        var imported = 0;

        foreach (var bill in command.Bills)
        {
            var account = userAccounts.FirstOrDefault(a =>
                a.Name.Equals(bill.AccountName, StringComparison.OrdinalIgnoreCase));

            if (account == null)
            {
                errors.Add($"Account '{bill.AccountName}' not found for bill dated {bill.IssueDate}");
                continue;
            }

            // Check for duplicate by invoice number or issue date
            if (!String.IsNullOrWhiteSpace(bill.InvoiceNumber) &&
                account.Bills.Any(b => b.InvoiceNumber == bill.InvoiceNumber))
            {
                errors.Add($"Bill with invoice number '{bill.InvoiceNumber}' already exists for account '{account.Name}'");
                continue;
            }

            if (account.Bills.Any(b => b.IssueDate == bill.IssueDate))
            {
                errors.Add($"Bill dated {bill.IssueDate} already exists for account '{account.Name}'");
                continue;
            }

            var newBill = new Domain.Entities.Utility.Bill
            {
                AccountId = account.Id,
                Cost = bill.Cost,
                CostsIncludeGST = bill.CostsIncludeGST,
                CurrentReading = bill.CurrentReading,
                PreviousReading = bill.PreviousReading,
                IssueDate = bill.IssueDate,
                InvoiceNumber = bill.InvoiceNumber,
                Total = bill.Total,
                Discounts = bill.Discounts.Select(d => new Domain.Entities.Utility.Discount
                {
                    DiscountAmount = d.DiscountAmount,
                    DiscountPercent = d.DiscountPercent,
                    Reason = d.Reason,
                }).ToList(),
                Periods = bill.Periods.Select(p => new Domain.Entities.Utility.Period
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
            };

            account.Bills.Add(newBill);
            imported++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResult
        {
            Imported = imported,
            Failed = errors.Count,
            Errors = errors,
        };
    }
}
