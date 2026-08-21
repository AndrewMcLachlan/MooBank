using System.ComponentModel;

namespace Asm.MooBank.Modules.Bills.Models;

/// <summary>
/// A kind of service charge that can appear on a bill.
/// </summary>
[Description("A kind of service charge that can appear on a bill.")]
public record ChargeType
{
    [Description("The ID of the charge type.")]
    public int Id { get; init; }

    [Description("The name of the charge type.")]
    public string Name { get; init; } = String.Empty;

    [Description("The utility this charge applies to, or null where it applies to any.")]
    public UtilityType? UtilityType { get; init; }
}

public static class ChargeTypeExtensions
{
    public static ChargeType ToModel(this Domain.Entities.Utility.ChargeType chargeType) =>
        new()
        {
            Id = chargeType.Id,
            Name = chargeType.Name,
            UtilityType = chargeType.UtilityType,
        };

    public static IEnumerable<ChargeType> ToModel(this IEnumerable<Domain.Entities.Utility.ChargeType> chargeTypes) =>
        chargeTypes.Select(c => c.ToModel());
}
