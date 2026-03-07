using System.Diagnostics.CodeAnalysis;

namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Bill", Schema = "utilities")]
public class Bill
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required Guid AccountId { get; set; }

    [StringLength(11)]
    public string? InvoiceNumber { get; set; }

    public required DateOnly IssueDate { get; set; }

    public int? CurrentReading { get; set; }

    public int? PreviousReading { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int? Total { get; set; } // Computed column

    [Column(TypeName = "bit")]
    public bool? CostsIncludeGST { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? Cost { get; set; } // Computed column

    [ForeignKey("AccountId")]
    [AllowNull]
    public virtual Account Account { get; set; }

    public virtual ICollection<Discount> Discounts { get; set; } = [];

    public virtual ICollection<Period> Periods { get; set; } = [];
}
