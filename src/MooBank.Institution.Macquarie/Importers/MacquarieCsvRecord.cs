using CsvHelper.Configuration.Attributes;

namespace Asm.MooBank.Institution.Macquarie.Importers;

internal class MacquarieCsvRecord
{
    [Index(0)]
    [Name("Transaction Date")]
    public string TransactionDate { get; set; } = String.Empty;

    [Index(1)]
    [Name("Details")]
    public string Details { get; set; } = String.Empty;

    [Index(2)]
    [Name("Account")]
    public string Account { get; set; } = String.Empty;

    [Index(3)]
    [Name("Category")]
    public string Category { get; set; } = String.Empty;

    [Index(4)]
    [Name("Subcategory")]
    public string Subcategory { get; set; } = String.Empty;

    [Index(5)]
    [Name("Tags")]
    public string Tags { get; set; } = String.Empty;

    [Index(6)]
    [Name("Notes")]
    public string Notes { get; set; } = String.Empty;

    [Index(7)]
    [Name("Debit")]
    public string Debit { get; set; } = String.Empty;

    [Index(8)]
    [Name("Credit")]
    public string Credit { get; set; } = String.Empty;

    [Index(9)]
    [Name("Balance")]
    public string Balance { get; set; } = String.Empty;

    [Index(10)]
    [Name("Original Description")]
    public string OriginalDescription { get; set; } = String.Empty;
}
