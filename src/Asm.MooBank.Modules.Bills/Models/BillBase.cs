namespace Asm.MooBank.Modules.Bills.Models;

public abstract record BillBase
{
    public string? InvoiceNumber { get; set; }

    public DateOnly IssueDate { get; set; }

    public int? CurrentReading { get; set; }

    public int? PreviousReading { get; set; }

    public int Total { get; set; } // Computed column

    public bool? CostsIncludeGST { get; set; }

    public decimal Cost { get; set; }

    public IEnumerable<Period> Periods { get; set; } = [];

    public IEnumerable<Discount> Discounts { get; set; } = [];
}
