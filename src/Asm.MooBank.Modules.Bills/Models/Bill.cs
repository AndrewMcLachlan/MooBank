using System.ComponentModel.DataAnnotations.Schema;

namespace Asm.MooBank.Modules.Bills.Models;

public record Bill : BillBase
{
    public int Id { get; init; }

}

public static class BillExtensions
{
    public static Bill ToModel(this Asm.MooBank.Domain.Entities.Utility.Bill entity)
    {
        return new Bill
        {
            Id = entity.Id,
            InvoiceNumber = entity.InvoiceNumber,
            IssueDate = entity.IssueDate,
            CurrentReading = entity.CurrentReading,
            PreviousReading = entity.PreviousReading,
            Total = entity.Total,
            CostsIncludeGST = entity.CostsIncludeGST,
            Cost = entity.Cost,
            Periods = entity.Periods.ToModel(),
        };
    }

    public static IEnumerable<Bill> ToModel(this IEnumerable<Asm.MooBank.Domain.Entities.Utility.Bill> entities)
    {
        return entities.Select(entity => entity.ToModel());
    }
}
