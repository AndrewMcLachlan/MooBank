namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Discount", Schema = "utilities")]
public class Discount : Entity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public byte? DiscountPercent { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal? DiscountAmount { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = [];

    [NotMapped]
    public bool IsValid => (DiscountPercent.HasValue || DiscountAmount.HasValue) && !(DiscountPercent.HasValue && DiscountAmount.HasValue);
}
