namespace Asm.MooBank.Domain.Entities.Utility;

[Table("ServiceCharge", Schema = "utilities")]
public class ServiceCharge
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PeriodId { get; set; }

    [Required]
    [Column(TypeName = "decimal(12, 5)")]
    public decimal ChargePerDay { get; set; }

    [ForeignKey("PeriodId")]
    public virtual Period? Period { get; set; }
}
