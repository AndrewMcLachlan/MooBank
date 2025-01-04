namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Usage", Schema = "utilities")]
public class Usage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int PeriodId { get; set; }

    [Required]
    [Column(TypeName = "decimal(5, 5)")]
    public decimal PricePerUnit { get; set; }

    [Required]
    [Column(TypeName = "decimal(7, 3)")]
    public decimal TotalUsage { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal Cost { get; set; } // Computed column

    [ForeignKey("PeriodId")]
    public virtual Period? Period { get; set; }
}
