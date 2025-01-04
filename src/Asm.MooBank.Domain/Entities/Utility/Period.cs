using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Period", Schema = "utilities")]
public class Period : Entity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int BillId { get; set; }

    [Required]
    public DateTime PeriodStart { get; set; }

    [Required]
    public DateTime PeriodEnd { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int DaysInclusive { get; set; } // Computed column

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int Days { get; set; } // Computed column

    [AllowNull]
    public virtual ServiceCharge ServiceCharge { get; set; }

    [AllowNull]
    public virtual Usage Usage { get; set; }

    [AllowNull]
    [ForeignKey("BillId")]
    public virtual Bill Bill { get; set; }
}
