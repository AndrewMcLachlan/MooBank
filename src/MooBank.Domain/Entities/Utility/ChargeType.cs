namespace Asm.MooBank.Domain.Entities.Utility;

/// <summary>
/// What a service charge is for. A water bill carries two: water supply and sewerage.
/// </summary>
[Table("ChargeType", Schema = "utilities")]
public class ChargeType
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public required string Name { get; set; }

    /// <summary>
    /// Null applies to any utility; set, it keeps sewerage off an electricity bill.
    /// </summary>
    [Column("UtilityTypeId")]
    public UtilityType? UtilityType { get; set; }
}
