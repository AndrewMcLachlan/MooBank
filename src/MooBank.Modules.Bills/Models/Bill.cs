namespace Asm.MooBank.Modules.Bills.Models;

public record Bill : BillBase
{
    public int Id { get; init; }

    public Guid AccountId { get; init; }

    public string AccountName { get; init; } = String.Empty;
}

public static class BillExtensions
{
    public static Bill ToModel(this Asm.MooBank.Domain.Entities.Utility.Bill entity)
    {
        return new Bill
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            AccountName = entity.Account?.Name ?? String.Empty,
            InvoiceNumber = entity.InvoiceNumber,
            IssueDate = entity.IssueDate,
            CurrentReading = entity.CurrentReading,
            PreviousReading = entity.PreviousReading,
            Total = entity.Total,
            CostsIncludeGST = entity.CostsIncludeGST,
            Cost = entity.Cost,
            Periods = entity.Periods.ToModel(),
            Discounts = entity.Discounts.ToModel(),
        };
    }

    public static IEnumerable<Bill> ToModel(this IEnumerable<Asm.MooBank.Domain.Entities.Utility.Bill> entities)
    {
        return entities.Select(entity => entity.ToModel());
    }
}
