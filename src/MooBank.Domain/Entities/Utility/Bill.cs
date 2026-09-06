using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Domain.Entities.Utility;

[Table("Bill", Schema = "utilities")]
[PrimaryKey(nameof(Id))]
public class Bill(int id) : KeyedEntity<int>(id)
{
    public Bill() : this(default) { }

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

    /// <summary>
    /// Replaces the bill's details with those supplied.
    /// </summary>
    /// <remarks>
    /// Periods -- and the usages and service charges within them -- are replaced wholesale rather
    /// than matched up and edited in place. Nothing outside a bill refers to a period, a usage or a
    /// service charge, so there is no identity worth preserving, and matching them would mean
    /// carrying ids through the API for no gain.
    ///
    /// <see cref="Total"/> and <see cref="Cost"/> are computed by the database and so are not
    /// settable here: readings drive the first and the periods drive the second.
    /// </remarks>
    public void Update(string? invoiceNumber, DateOnly issueDate, int? currentReading, int? previousReading, bool? costsIncludeGST, IEnumerable<Period> periods, IEnumerable<Discount> discounts)
    {
        InvoiceNumber = invoiceNumber;
        IssueDate = issueDate;
        CurrentReading = currentReading;
        PreviousReading = previousReading;
        CostsIncludeGST = costsIncludeGST;

        Periods.Clear();
        foreach (var period in periods)
        {
            Periods.Add(period);
        }

        Discounts.Clear();
        foreach (var discount in discounts)
        {
            Discounts.Add(discount);
        }
    }
}
