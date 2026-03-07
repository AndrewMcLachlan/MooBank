namespace Asm.MooBank.Modules.Bills.Models;

public record Discount
{
    public byte? DiscountPercent { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? Reason { get; set; }
}


public static class DiscountExtensions
{
    public static Discount ToModel(this Domain.Entities.Utility.Discount discount) =>
        new()
        {
            DiscountPercent = discount.DiscountPercent,
            DiscountAmount = discount.DiscountAmount,
            Reason = discount.Reason,
        };

    public static IEnumerable<Discount> ToModel(this IEnumerable<Domain.Entities.Utility.Discount> discounts) =>
        discounts.Select(p => p.ToModel());
}
